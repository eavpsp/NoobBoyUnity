using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Status 
{
   public enum ColorModes
    {
        NORMAL = 0,
        RETRO = 1,
        GRAY = 2
    };

    public class _Status
    {
        public bool debug = false;
        public bool isRunning = true;
        public bool isPaused = false;
        public bool doStep = true;
        public bool soundEnabled = false;
        public int colorMode = 0;
    };
   
    
    

    

}
