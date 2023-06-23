using CoolishUI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;

public class ScanController : Singleton<ScanController>
{
    [SerializeField] private float _scanningTime = 5f;
    [SerializeField] private ARMeshManager _arMeshManager;
    [SerializeField] private ARCameraManager _arCameraManager;
    [SerializeField] private GameObject _modelViewer;
    [SerializeField] private Transform _modelViewParent;
    [SerializeField] private SimpleDebugConsole _console;
    [SerializeField] private float _getScreenTime = .5f;

    private bool _isScanning = false;
    private List<MeshData> _datas = new List<MeshData>();
    private float _getScreenTimeTemp = 1f;
    private Coroutine _scanning;


    protected override void Awake()
    {
        base.Awake();
        _getScreenTimeTemp = _getScreenTime;
        _arMeshManager.enabled = false;
        _arMeshManager.density = 1f;
    }

    private void OnEnable()
    {
        _arMeshManager.meshesChanged += OnMeshesChanged;
    }

    private void OnDisable()
    {
        _arMeshManager.meshesChanged -= OnMeshesChanged;
    }

    private void Update()
    {
        _getScreenTimeTemp += Time.deltaTime;
    }

    public void ScanStart()
    {
        if (!_isScanning)
        {
            _arMeshManager.enabled = true; // �������� ARMeshManager ��� ������������ �����
            XRMeshSubsystem arMeshSubsystem = (XRMeshSubsystem)_arMeshManager.subsystem; // �������� ������ � ���������� ARKitMeshSubsystem

            if (arMeshSubsystem != null)
            {
                arMeshSubsystem.Start();
                _isScanning = true;
                StartCoroutine(Scaning());
                Debug.Log("Scan START");
            }
        }
    }

    IEnumerator Scaning()
    {
        yield return new WaitForSeconds(_scanningTime);

        if (!_isScanning)
            yield break;

        ScanStop();
        //var sccreenShot = ScreenCapture.CaptureScreenshotAsTexture();
        //yield return new WaitForSeconds(2f);
        //foreach (var meshFilter in _arMeshManager.meshes)
        //{
        //    meshFilter.GetComponent<MeshRenderer>().enabled = true;
        //    meshFilter.TextureMesh(sccreenShot);
        //}
        Debug.Log("Done");
    }

    public void ScanStop()
    {
        if (_isScanning)
        {
            StartCoroutine(Stopping());
            //Camera.main.enabled = false;
            //_arMeshManager.enabled = false; // ��������� ARMeshManager

            //XRMeshSubsystem arMeshSubsystem = (XRMeshSubsystem)_arMeshManager.subsystem;

            //ToogleMeshes(false);
            //UIController.Instance.HideUI();
            //var screenShot = ScreenCapture.CaptureScreenshotAsTexture();
            //foreach (var data in _datas)
            //{
            //    data.Texture = screenShot;
            ////}
            //ToogleMeshes(true);
            //UIController.Instance.ShowUI();

            //UIController.Instance.ShowViewerPanel();

            //foreach (var meshFilter in _arMeshManager.meshes)
            //{
            //    meshFilter.transform.SetParent(_modelViewParent, false);
            //}

            //_modelViewer.SetActive(true);
            //if (arMeshSubsystem != null)
            //{
            //    arMeshSubsystem.Stop();
            //    _arCameraManager.enabled = false;
            //    _isScanning = false;
            //    Debug.Log($"Scan STOP. Meshes: {_arMeshManager.meshes.Count} Datas: {_datas.Count}");
            //}
        }
    }

    IEnumerator Stopping()
    {
        yield return new WaitForEndOfFrame();
        //UIController.Instance.HideUI();
        _arMeshManager.enabled = false; // ��������� ARMeshManager

        //XRMeshSubsystem arMeshSubsystem = (XRMeshSubsystem)_arMeshManager.subsystem;

        //if (arMeshSubsystem != null)
        //{
        //    arMeshSubsystem.Stop();
        //    _isScanning = false;
        //}

        //foreach (var meshFilter in _arMeshManager.meshes)
        //{
        //    meshFilter.GetComponent<MeshRenderer>().enabled = false;
        //}
        //yield return new WaitForSeconds(1f);

        //var screenShot = ScreenCapture.CaptureScreenshotAsTexture();
        //yield return new WaitForSeconds(1f);
        //foreach (var filter in _arMeshManager.meshes)
        //{
        //    filter.GetComponent<MeshRenderer>().enabled = true;
        //    filter.TextureMesh(screenShot);
        //}

        foreach (var meshFilter in _arMeshManager.meshes)
        {
            meshFilter.transform.SetParent(_modelViewParent, false);
        }

        //yield return new WaitForSeconds(1f);
        _arCameraManager.enabled = false;
        UIController.Instance.ShowViewerPanel();
    }

    private void OnMeshesChanged(ARMeshesChangedEventArgs eventArgs)
    {
        foreach (var meshFilter in eventArgs.added)
        {
            CreateMeshObject(meshFilter);
        }

        foreach (var meshFilter in eventArgs.updated)
        {
            UpdateMeshObject(meshFilter);
        }

        foreach (var meshFilter in eventArgs.removed)
        {
            RemoveMeshObject(meshFilter);
        }
    }

    private void CreateMeshObject(MeshFilter meshFilter)
    {
        Debug.Log($"Mesh create. {_arMeshManager.meshes.Count}");
        SaveCameraTextureToMesh(meshFilter);
    }

    //private MeshFilter CreateMesh(MeshFilter meshFilter)
    //{
    //    var mesh = meshFilter.mesh;
    //    // ��������� ������ ����
    //    Vector3[] vertices = new Vector3[mesh.vertices.Length];
    //    mesh.vertices.CopyTo(vertices, 0);

    //    // ��������� ������������� ����
    //    int[] triangles = new int[mesh.triangles.Length];
    //    mesh.triangles.CopyTo(triangles, 0);

    //    GameObject meshObject = Instantiate(meshPrefab, Vector3.zero, Quaternion.identity);

    //    // �������� ������ ���� �������
    //    Mesh meshComponent = new Mesh();
    //    meshComponent.vertices = vertices;
    //    meshComponent.triangles = triangles;
    //    meshObject.GetComponent<MeshFilter>().mesh = meshComponent;
    //    meshObject.GetComponent<MeshRenderer>().material = meshPrefab.GetComponent<MeshRenderer>().sharedMaterial;

    //    // ������������ ������� � ������������
    //    meshObject.transform.position = meshFilter.transform.position;
    //    meshObject.transform.rotation = meshFilter.transform.rotation;
    //    meshObject.transform.localScale = Vector3.one;
    //    return 
    //}

    private void UpdateMeshObject(MeshFilter meshFilter)
    {
        _getScreenTimeTemp += Time.deltaTime;
        if (_getScreenTimeTemp >= _getScreenTime)
        {
            SaveCameraTextureToMesh(meshFilter);
            _getScreenTimeTemp = 0f;
        }

        if (meshFilter == null)
        {
            Debug.LogError("Missing MeshFilter component.");
            return;
        }
    }

    private void RemoveMeshObject(MeshFilter meshFilter)
    {
        if (meshFilter == null)
        {
            Debug.LogError("Missing MeshFilter component.");
            return;
        }

        var data = _datas.FirstOrDefault(data => data.MeshFilter == meshFilter);
        if (data != null)
            _datas.Remove(data);

        Destroy(meshFilter.gameObject);
    }

    IEnumerator SaveScreen(MeshFilter meshFilter)
    {
        ToogleMeshes(false);
        UIController.Instance.HideUI();
        var screenShoot = ScreenCapture.CaptureScreenshotAsTexture();

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        var data = _datas.FirstOrDefault((data) => data.MeshFilter == meshFilter);
        if (data != null)
        {
            data.Texture = screenShoot;
            Debug.Log("Update Screen");
        }
        else
        {
            data = new MeshData(meshFilter, screenShoot);
            Debug.Log("Create Screen");
            _datas.Add(data);
        }


        UIController.Instance.ShowUI();
        ToogleMeshes(true);
        _scanning = null;
    }

    private void SaveCameraTextureToMesh(MeshFilter meshFilter)
    {
        Debug.Log("Save Screen");
        if (_scanning == null)
            _scanning = StartCoroutine(SaveScreen(meshFilter));
    }

    public void ConvertToModel()
    {
        SetTextures();
    }

    public void SetTextures()
    {
        if (_datas == null || _datas.Count == 0)
        {
            Debug.Log("No DATAS");
            return;
        }

        Debug.Log($"Convert Meshes: {_arMeshManager.meshes.Count}. Datas: {_datas.Count}");
        foreach (var data in _datas)
        {
            data.MeshFilter.TexturedMesh(data.Texture);
        }
    }

    private void ToogleMeshes(bool activate)
    {
        _console.enabled = activate;
        if (_arMeshManager.meshes == null || _arMeshManager.meshes.Count <= 0)
            return;

        foreach (var mesh in _arMeshManager.meshes)
        {
            mesh.GetComponent<MeshRenderer>().enabled = activate;
        }
    }
}
