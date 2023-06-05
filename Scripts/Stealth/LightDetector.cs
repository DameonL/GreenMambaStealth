using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace GreenMambaStealth.Stealth
{
	/// <summary>
	/// Uses a specialized camera to render to a RenderTexture, then analyzes it to determine ambient light level.
	/// </summary>
	public class LightDetector : MonoBehaviour
	{
		/// <summary>
		/// The RenderTexture this detector is drawing off of.
		/// </summary>
		[Tooltip("A RenderTexture to use as a template for this detector.")]
		[SerializeField]
		private RenderTexture _renderTexture;

		/// <summary>
		/// The camera this detector is associated with.
		/// </summary>
		[Tooltip("The camera this detector is using. If left empty, the detector will look for a Camera component on the same transform.")]
		[SerializeField]
		private Camera _targetCamera;

		/// <summary>
		/// A RawImage to render to in a UI, for debugging purposes.
		/// </summary>
		[Tooltip("A RawImage to render to in a UI for debugging. To use, create a GameObject as a child of a Canvas, and add a RawImage component.")]
		[SerializeField]
		private RawImage _debugRender;

		/// <summary>
		/// Increases threads used for processing light.
		/// </summary>
		[Tooltip("Increases threads used for processing light.\nHigher numbers increase performance of this script, but may hurt other performance.")]
		[SerializeField]
		[Range(1, 15)]
		private int _threads = 2;

		/// <summary>
		/// Time in milliseconds threads sleep between each processing cycle.
		/// </summary>
		[Tooltip("Time in milliseconds threads sleep before processing again.\nHigher numbers will increase performance, but decrease update speed.")]
		[SerializeField]
		[Range(1, 1000)]
		private int _threadSleepTime = 1000 / 60;

		private Color[] _pixelArray;
		private Texture2D _detectionTexture;
		private int _collisions;
		private int _chunkSize = 1;
		private int _threadsComplete;
		private List<LightDetectorWorker> _workers = new List<LightDetectorWorker>();
		private Action _threadCompleteHandler;

		/// <summary>
		/// A measurement of the intensity of the lighting hitting this detector.
		/// </summary>
		public float Intensity { get; private set; }

		private void Awake()
		{
			if (_targetCamera == null)
			{
				_targetCamera = GetComponent<Camera>();
			}

			if (_targetCamera == null)
			{
				throw new UnityEngine.MissingComponentException("No Camera found for LightDetector. Please add a Camera component, or assign one.");
			}

			_detectionTexture = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.ARGB32, false);
			_renderTexture = new RenderTexture(_renderTexture);
			_targetCamera.targetTexture = _renderTexture;

			if (_debugRender != null)
				_debugRender.texture = _renderTexture;
		}

		private void OnEnable()
		{
			PrepareThreads();
		}

		private void OnDisable()
		{
			foreach (var worker in _workers)
			{
				worker.StopThread();
			}
		}

		private void PrepareThreads()
		{
			_pixelArray = new Color[_detectionTexture.width * _detectionTexture.height];
			_chunkSize = _pixelArray.Length / _threads;
			_threadCompleteHandler = () => { Interlocked.Increment(ref _threadsComplete); };

			for (var i = 0; i < _threads; i++)
			{
				int start = i * _chunkSize;
				int end = (i + 1) * _chunkSize;
				LightDetectorWorker worker = new LightDetectorWorker(_threadSleepTime, _threadCompleteHandler, start, end, _pixelArray);
				_workers.Add(worker);
			}

			foreach (var worker in _workers)
			{
				worker.StartThread();
			}
		}

		private void Update()
		{
			if (_collisions == 0)
			{
				if (_threadsComplete >= _threads)
				{
					_threadsComplete = 0;
					float intensity = 0;
					int validThreads = 0;
					for (var i = 0; i < _workers.Count; i++)
					{
						if (_workers[i].ChunkAverage > 0)
						{
							intensity += _workers[i].ChunkAverage;
							validThreads++;
						}
					}
					intensity /= validThreads;
					Intensity = intensity;

					FillPixels();
					ProcessPixels();
				}
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if ((!other.isTrigger) && (other.transform.root != transform.root))
				_collisions++;
		}

		private void OnTriggerExit(Collider other)
		{
			if ((!other.isTrigger) && (other.transform.root != transform.root))
				_collisions--;
		}

		private void FillPixels()
		{
			RenderTexture.active = _renderTexture;
			_detectionTexture.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0, false);
			_detectionTexture.Apply();

			int x = 0;
			int y = 0;
			for (y = 0; y < _detectionTexture.height; y++)
			{
				for (x = 0; x < _detectionTexture.width; x++)
				{
					_pixelArray[x * y] = _detectionTexture.GetPixel(x, y);
				}
			}
		}

		private void ProcessPixels()
		{
			for (var i = 0; i < _workers.Count; i++)
			{
				_workers[i].ThreadPaused = false;
			}

		}

		/// <summary>
		/// Repositions the light detector to be sure it has the best view of the sphere used for detection.
		/// </summary>
		public void Calibrate()
		{
			int layer = LayerMask.NameToLayer("LightDetection");
			if (layer == -1)
			{
				throw new Exception("LightDetector unable to locate LightDetection layer. Please add a Layer named LightDetection to your project.");
			}
			gameObject.layer = layer;
			transform.parent.gameObject.layer = layer;
			transform.parent.localPosition = Vector3.zero;

			transform.localScale = Vector3.one;
			_detectionTexture = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.ARGB32, false);
			Vector3 cachedPosition = transform.position;
			MeshRenderer renderer = transform.parent.GetComponentInChildren<MeshRenderer>();
			Vector3 directionFromCharacter = (transform.position - transform.parent.position).normalized;

			directionFromCharacter.Scale(renderer.bounds.extents);
			transform.position = transform.parent.position + directionFromCharacter;
			transform.LookAt(renderer.transform);
			Camera camera = GetComponent<Camera>();
			camera.orthographicSize = renderer.bounds.extents.x;
			camera.nearClipPlane = 0;
			camera.farClipPlane = renderer.bounds.extents.x;
			_detectionTexture = null;
		}

		private class LightDetectorWorker
		{
			private Action _completeHandler { get; set; }
			private int _arrayStart { get; set; }
			private int _arrayEnd { get; set; }
			private Color[] _pixels { get; set; }
			private bool ThreadRunning { get; set; }
			private int _sleepTime = 116;
			private Thread _activeThread;

			/// <summary>
			/// The average light intensity of this chunk.
			/// </summary>
			public float ChunkAverage { get; set; }
			/// <summary>
			/// Whether or not the thread is paused. Every loop, this is set to
			/// true, and must be set to false to continue processing.
			/// </summary>
			public bool ThreadPaused { get; set; }

			public LightDetectorWorker(int sleepTime, Action completeHandler, int arrayStart, int arrayEnd, Color[] pixels)
			{
				_completeHandler = completeHandler;
				_arrayStart = arrayStart;
				_arrayEnd = arrayEnd;
				_pixels = pixels;
				_sleepTime = sleepTime;
			}

			/// <summary>
			/// Begin working on a separate thread.
			/// </summary>
			public void StartThread()
			{
				ThreadRunning = true;
				_activeThread = new Thread(RunThread);
				_activeThread.IsBackground = true;
				_activeThread.Start();
			}

			/// <summary>
			/// Attempt to gently stop the thread.
			/// </summary>
			public void StopThread()
			{
				ThreadRunning = false;
				ThreadPaused = false;
				_activeThread = null;
			}

			/// <summary>
			/// Abort the thread immediately.
			/// </summary>
			public void StopThreadImmediate()
			{
				if (_activeThread != null)
				{
					_activeThread.Abort();
				}
			}

			private void RunThread()
			{
				try
				{
					while (ThreadRunning)
					{
						ChunkAverage = 0;
						int countedPixels = 0;
						for (int i = _arrayStart; i < _arrayEnd; i++)
						{
							if (_pixels[i].a > 0.01)
							{
								ChunkAverage += (_pixels[i].r + _pixels[i].g + _pixels[i].b) / 3;
								countedPixels++;
							}
						}

						if ((ChunkAverage != 0) && (countedPixels != 0))
							ChunkAverage /= countedPixels;

						_completeHandler.Invoke();
						ThreadPaused = true;

						while (ThreadPaused)
							Thread.Sleep(_sleepTime);
					}
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}
			}

		}
	}
}
