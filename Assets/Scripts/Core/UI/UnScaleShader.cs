using UnityEngine;

public class UnScaleShader : MonoBehaviour
{
    void Start()
    {
        
    }

 
    void Update()
    {
        Shader.SetGlobalFloat("_UnScaleTime", Time.unscaledTime);
    }
}