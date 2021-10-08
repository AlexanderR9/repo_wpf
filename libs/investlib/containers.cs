using System;
using investlib.structs;
using System.Collections.Generic;


namespace investlib.containers
{
    public class CFDContainer
    {
        public CFDContainer()
        {
            m_data = new List<CFDData>();
        }

        private List<CFDData> m_data;
        //public List<CFDData> cfdList {get; private set{}}

    }
}
