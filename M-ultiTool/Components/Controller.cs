using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiTool.Components
{
    internal class Controller : MonoBehaviour
    {
        public void LateUpdate()
        {
            MultiTool.LateUpdate();
        }
    }
}
