using System.Collections.Generic;
using UnityEngine;

namespace InstancedIndirect
{
    public class IndirectRendererManager : MonoBehaviour
    {
        public List<IndirectRenderer> renderers;
        public bool doRender = true;

        void LateUpdate()
        {
            if (!doRender)
            {
                return;
            }

            Render();
        }

        void Render()
        {
            if (renderers == null || renderers.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < renderers.Count; i++)
            {
                IndirectRenderer ir = renderers[i];

                if(ir)
                {
                    ir.Render();
                }
            }
        }
    }
}
