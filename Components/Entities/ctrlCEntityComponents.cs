﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace ctrlC.Components.Entities
{
    public struct CtrlCObject : IComponentData
    {
        public int m_Priority;
    }
}
