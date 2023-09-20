using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;


public class AR_Cursor : MonoBehaviour
{

    public bool useCursor;
    public GameObject cursorChildObject;
    public GameObject objectToPlace;
    public GameObject bannerToPlace;
    public ARRaycastManager raycastManager;
    public MyGameManager myGameManager;

    [SerializeField]
    private Camera arCam;

    [SerializeField]
    private bool displayCanvas = true;

    [SerializeField]
    private GameObject canvasToDisplay;

    [SerializeField]
    private Color activeColor = Color.red;

    [SerializeField]
    private Color inactiveColor = Color.gray;

    [SerializeField]
    private Button dismissButton;

    GameObject spawnedObject = null;

    List<ARRaycastHit> hits = new List<ARRaycastHit>();



    // Start is called before the first frame update
    void Start()
    {
        
        cursorChildObject.SetActive(useCursor);
    }

    // Update is called once per frame
    void Update()
    {
        if (useCursor)
        {
            UpdateCursor();
        }

        if (raycastManager.Raycast(Input.GetTouch(0).position, hits)){

            if(Input.GetTouch(0).phase == TouchPhase.Began && spawnedObject == null)
            {
                RaycastHit hit;
                Ray ray = arCam.ScreenPointToRay(Input.GetTouch(0).position);
                if (Physics.Raycast(ray, out hit))
                {
                    if(hit.collider.gameObject.tag == "ARObject")
                    {
                        spawnedObject = hit.collider.gameObject;
                        PlacementObject placementObject = hit.transform.GetComponent<PlacementObject>();
                        MeshRenderer placementObjectMeshRenderer = bannerToPlace.GetComponent<MeshRenderer>();
                        if (placementObject != null)
                        {
                            placementObject.Selected = true;
                            placementObjectMeshRenderer.material.color = activeColor;
                            placementObject.GetComponentInChildren<Canvas>();

                            if (displayCanvas)
                            {
                                placementObject.ToggleCanvas(true);
                                placementObject.ToggleOverlay();
                            }

                        }

                    }
                    else
                    {
                        GameObject.Instantiate(bannerToPlace, hits[0].pose.position, hits[0].pose.rotation);
                        PlacementObject placementObject = bannerToPlace.GetComponent<PlacementObject>();
                        MeshRenderer placementObjectMeshRenderer = bannerToPlace.GetComponent<MeshRenderer>();
                        if (placementObject != null)
                        {
                            placementObject.Selected = false;
                            placementObjectMeshRenderer.material.color = inactiveColor;

                            if (displayCanvas)
                            {
                                placementObject.ToggleCanvas(false);
                            }
                        }
                    }

                }
            }
            if(Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                spawnedObject = null;

            }
        }
    }
    void UpdateCursor()
    {
        Vector2 screenPosition = Camera.main.ViewportToScreenPoint(new Vector2(0.5f, 0.5f));
        raycastManager.Raycast(screenPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes);

        if (hits.Count > 0)
        {
            transform.position = hits[0].pose.position;
            transform.rotation = hits[0].pose.rotation;
        }
    }

}
