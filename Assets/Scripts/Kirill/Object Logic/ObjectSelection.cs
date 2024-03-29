using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;

public class ObjectSelection : MonoBehaviour
{
    [SerializeField] private bool isCameraMove;
    [SerializeField] private MonoBehaviour scriptToDisable;

    private MeshRenderer _meshRenderer;
    private Camera _mainCamera;
    private ObjectSelection[] _selectObjects;

    private GameObject _vcamObject;

    [NonSerialized] public bool isSelected;
    [NonSerialized] public bool isTransitioning;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _mainCamera = Camera.main;
        _selectObjects = FindObjectsOfType<ObjectSelection>();
    }

    private void Start()
    {
        DeselectAll();
        SetMoving(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && CheckClick() && !GameManager.Instance.isInTheMouseTrap)
        {
            if (!isSelected)
                Select();
            else
                Deselect();
        }
        
        if (Input.GetKeyDown(KeyCode.E))
            DeselectAll();
    }

    private bool CheckClick()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                return true;
            }
        }

        return false;
    }

    private void Select()
    {
        List<Material> materials = _meshRenderer.materials.ToList();

        if (!materials.Contains(GameManager.Instance.glowMaterial))
        {
            materials.Add(GameManager.Instance.glowMaterial);

            if (isCameraMove)
                SetCamera();

            foreach (ObjectSelection selectObject in _selectObjects)
            {
                if (selectObject.isSelected && selectObject != this)
                    selectObject.Deselect();
            }

            _meshRenderer.SetMaterials(materials);
            isSelected = true;
            
            SetMoving(true);

            StartCoroutine(SetRotatingAndCross(true));
        }
    }

    private void SetCamera()
    {
        _vcamObject = Instantiate(GameManager.Instance.cameraPrefab, transform.position, Quaternion.identity);
        CinemachineFreeLook vcam = _vcamObject.GetComponent<CinemachineFreeLook>();

        bool hasStabilizator = false;
        CameraStabilizeObject stabil = null;
        foreach (Transform o in transform)
        {
            if (o.TryGetComponent(out stabil))
            {
                hasStabilizator = true;
                break;
            }
        }
        
        if (!hasStabilizator)
        {
            vcam.Follow = transform;
            vcam.LookAt = transform;
        }
        else
        {
            vcam.Follow = stabil.transform;
            vcam.LookAt = stabil.transform;
        }

        vcam.Priority = 11;
    }

    public void Deselect()
    {
        List<Material> materials = _meshRenderer.materials.ToList();
        materials.Remove(materials[^1]);
        
        ObjectAiming.RemoveAimGlow(materials);

        _meshRenderer.SetMaterials(materials);
        isSelected = false;

        _vcamObject.GetComponent<CinemachineFreeLook>().Priority = 0;

        Destroy(_vcamObject, 1.5f);
        
        SetMoving(false);
        StartCoroutine(SetRotatingAndCross(false));
    }

    public void DeselectAll()
    {
        foreach (var selectObject in _selectObjects)
        {
            if (selectObject.isSelected)
                selectObject.Deselect();
        }
    }

    private void SetMoving(bool isMoving)
    {
        scriptToDisable.enabled = isMoving;
    }
    
    private void LockCamera()
    {
        _vcamObject.GetComponent<CinemachineFreeLook>().m_XAxis.m_MaxSpeed = 0;
        _vcamObject.GetComponent<CinemachineFreeLook>().m_YAxis.m_MaxSpeed = 0;
    }
    
    private void UnlockCamera()
    {
        _vcamObject.GetComponent<CinemachineFreeLook>().m_XAxis.m_MaxSpeed = 300;
        _vcamObject.GetComponent<CinemachineFreeLook>().m_YAxis.m_MaxSpeed = 2;
    }

    private IEnumerator SetRotatingAndCross(bool flag)
    {
        LockCamera();
        if (!_selectObjects.Any(selectObject => selectObject.isSelected) && !flag)
            GameManager.Instance.cross.SetActive(false);
        
        isTransitioning = true;
        
        yield return new WaitForSeconds(1);
            
        isTransitioning = false;
        UnlockCamera();
        if(flag)
            Cursor.lockState = CursorLockMode.Locked;
        
        if (!_selectObjects.Any(selectObject => selectObject.isSelected) && !flag)
            Cursor.lockState = CursorLockMode.None;
            
        GameManager.Instance.cross.SetActive(flag);
    }
}