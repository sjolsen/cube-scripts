using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFx.Outline;

public class MouseInputController : MonoBehaviour
{
    public Camera MainCamera;
    public GameObject ClickIndicatorPrefab;

    private CameraController _cameraController;
    private List<GameObject> _clickIndicators = new List<GameObject>();
    private OutlineLayer _outlineLayer;
    private Vector3 _prevMousePosition = Vector3.zero;

    void Start()
    {
        _cameraController = MainCamera.gameObject.GetComponent<CameraController>();

        var outlineEffect = Camera.main.GetComponent<OutlineEffect>();
        _outlineLayer = new OutlineLayer("MyOutlines");

        _outlineLayer.OutlineColor = Color.white;
        _outlineLayer.OutlineWidth = 7;
        _outlineLayer.OutlineRenderMode = OutlineRenderFlags.Blurred | OutlineRenderFlags.EnableDepthTesting;
        _outlineLayer.MergeLayerObjects = true;

        outlineEffect.OutlineLayers.Add(_outlineLayer);
    }

    void Update()
    {
        var mouseDelta = Input.mousePosition - _prevMousePosition;
        _prevMousePosition = Input.mousePosition;
        if (_cameraController.Track.state == CameraController.GlobeTrack.State.GRABBED)
        {
            _cameraController.Track.UpdateAnglesDelta(mouseDelta.x, mouseDelta.y);
        }
        if (Input.GetMouseButtonDown(0))
        {
            Ray click = MainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(click, out RaycastHit hit))
            {
                _clickIndicators.Add(Instantiate(ClickIndicatorPrefab, hit.point, Quaternion.identity));
                _outlineLayer.Add(hit.collider.gameObject);
            }
            else if (_cameraController.Track.state != CameraController.GlobeTrack.State.GRABBED)
            {
                _cameraController.Track.Grab();
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            foreach (GameObject obj in _clickIndicators)
                Destroy(obj);
            _clickIndicators.Clear();
            if (_cameraController.Track.state == CameraController.GlobeTrack.State.GRABBED)
                _cameraController.Track.Release();
        }
        if (Input.GetMouseButtonDown(1))
        {
            _outlineLayer.Clear();
        }
    }
}
