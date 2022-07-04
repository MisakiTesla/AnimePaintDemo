using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintController : MonoBehaviour
{
    public LayerMask paintingBoardLayerMask;
    public Material drawerMaterial;
    public Texture2D regionTexture;
    // Start is called before the first frame update
    private int _hittedRegionIndex;
    private int _maxRegionIndex;
    private bool _isAnimating;
    private float _radius;
    void Start()
    {
        _maxRegionIndex = -1;
        drawerMaterial.SetInt("_SelectedRegion", _maxRegionIndex);
        drawerMaterial.SetFloat("_Radius", 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (_isAnimating)
        {
            _radius += Time.deltaTime;
            drawerMaterial.SetFloat("_Radius", _radius);
            if (_radius > 1)
            {
                _radius = 0;
                _isAnimating = false;
                _maxRegionIndex = _hittedRegionIndex+1;
                drawerMaterial.SetInt("_SelectedRegion", _maxRegionIndex);
                drawerMaterial.SetFloat("_Radius", _radius);
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray,out var hit , 100, paintingBoardLayerMask);

                if (hit.transform)
                {
                    var uv = hit.textureCoord;
                    Debug.Log($"uv = {uv}");

                    var color = regionTexture.GetPixel(Mathf.RoundToInt(uv.x * regionTexture.width),
                        Mathf.RoundToInt(uv.y * regionTexture.height));
                    Debug.Log(Mathf.RoundToInt(color.b*255));

                    _hittedRegionIndex = Mathf.RoundToInt(color.r*255) * 65536 + Mathf.RoundToInt(color.g*255) * 256 + Mathf.RoundToInt(color.b*255);
                    Debug.Log($"hittedRegionIndex = {_hittedRegionIndex} ,_maxRegionIndex = {_maxRegionIndex}");

                    // if (_hittedRegionIndex > _maxRegionIndex)
                    {
                        drawerMaterial.SetInt("_SelectedRegion", _hittedRegionIndex);
                        drawerMaterial.SetVector("_TouchPos", new Vector4(uv.x,uv.y,0,0));
                        _isAnimating = true;
                    }
                } 
            } 
        }



    }
}
