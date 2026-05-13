using UnityEngine;
using UnityEngine.Rendering;

namespace Modules.Outlines
{
    public class ShaderMaterial
    {
        private Material _silhouetteMaterial;
        private string shaderName;
        
        public ShaderMaterial(string shaderName)
        {
            this.shaderName = shaderName;
        }
        
        public Material GetMaterial()
        {
            if (_silhouetteMaterial != null) return _silhouetteMaterial;
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError($"Could not find shader {shaderName}");
                return null;
            }

            _silhouetteMaterial = CoreUtils.CreateEngineMaterial(shader);
            return _silhouetteMaterial;
        }
    }
}