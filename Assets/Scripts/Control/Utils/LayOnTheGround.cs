using UnityEngine;
using Utils;

/// <summary>
/// 强制一个 GameObject 贴地.
/// /// </summary>
[ExecuteAlways]
public class LayOnTheGround : MonoBehaviour
{
    [SerializeField] bool worldPos = true;
    
    [SerializeField] LayerMask mask = new LayerMask();
    
    void Update()
    {
        if(worldPos)
        {
            if(Physics.Raycast(this.transform.position + Vector3.up * 1e4f, Vector3.down, out var hit, 1e8f, mask))
            {
                this.transform.position = this.transform.position.Y(hit.point.y);
                
                // 转到法线方向.
                this.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
            else
            {
                this.transform.position = this.transform.position.Y(0);
            }
        }
        else 
        {
            this.transform.localPosition = this.transform.localPosition.Y(0);
        }
    }
}
