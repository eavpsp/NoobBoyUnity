using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using TMPro;
enum RegisterFlags { FLAG_ZERO = (1 << 7), FLAG_SUBTRACT = (1 << 6), FLAG_HALF_CARRY = (1 << 5), FLAG_CARRY = (1 << 4) };

public class Registers
{
    public ushort pc = 0; // Program Counter/Pointer
    public ushort sp; // Stack pointer
    public GameObject debugLabel;

    public struct RegisterAF
    {
        private ushort afValue;

        public byte f
        {
            get => (byte) (afValue & 0xFF);
            set
            {


                afValue = (ushort)((a << 8) | value);
            }

        }


        public byte a
        {
            get => (byte)((afValue >> 8) & 0xFF);
            set
            {


                afValue = (ushort)((value << 8) | f);
            }
        }

        public ushort af
        {
            get => afValue;
            set
            {
                this.afValue = value;
                a = (byte)((value >> 8) & 0xFF);
                f = (byte)(value & 0xFF);
            }
        }
      

    }
  
    public struct RegisterBC
    {
        private ushort bfVal;
        public byte c
        {
            get { return (byte)(bfVal & 0xFF); }
            set
            {

                bfVal = (ushort)((b << 8) | value);
                
            }

        }


        public byte b
        {
            get => (byte)((bfVal >> 8) & 0xFF);
            set
            {


                bfVal = (ushort)((value << 8) | c);
            }
        }

        public ushort bc
        {
            get => bfVal;
            set
            {
                this.bfVal = value;
                b = (byte)((value >> 8) & 0xFF);
                c = (byte)(value & 0xFF);
            }
        }
      
    }
   
    public struct RegisterDE
    {
        private ushort deval;
        public  byte e
        {
            get => (byte)(deval & 0xFF);
            set {


                deval = (ushort)((d << 8) | value);
            }

        }


        public byte d
        {
            get => (byte)((deval >> 8) & 0xFF);
            set
            {


                deval = (ushort)((value << 8) | e);
            }
        }

        public ushort de
        {
            get => deval;
            set
            {
                this.deval = value;
                d = (byte)((value >> 8) & 0xFF);
                e = (byte)(value & 0xFF);
            }
        }
        private void UpdateValue()
        {
            deval = (ushort)((d << 8) | e);
        }

    }

    public struct RegisterHL
    {
        private ushort hlval;
        public byte l
        {
            get => (byte)(hlval & 0xFF); 
            set
            {


                hlval = (ushort)((h << 8) | value);
            }

        }


        public byte h
        {
            get => (byte)((hlval >> 8) & 0xFF);
            set
            {


                hlval = (ushort)((value << 8) | l);
            }
        }

        public ushort hl
        {
            get => hlval;
            set
            {
                this.hlval = value;
                h = (byte)((value >> 8) & 0xFF);
                l = (byte)(value & 0xFF);
            }
        }
        
    }
    public RegisterAF AF = new RegisterAF();
    public RegisterDE DE = new RegisterDE();
    public RegisterHL HL = new RegisterHL();
    public RegisterBC BC = new RegisterBC();
    //FUNCTIONS

    public bool is_flag_set(byte flag)
    {
        return (this.AF.f & flag) != 0;
    }
    public void set_flags(byte flags, bool state)
    {
        this.AF.f = state ? (byte)(this.AF.f | flags) : (byte)(this.AF.f & ~flags);
    }
    public void print_flags()
    {
      
       Debug.Log("Z: " + is_flag_set((byte)RegisterFlags.FLAG_ZERO) + " N: " + is_flag_set((byte)RegisterFlags.FLAG_SUBTRACT) + " H: " + is_flag_set((byte)RegisterFlags.FLAG_HALF_CARRY) + " C: "  + is_flag_set((byte)RegisterFlags.FLAG_CARRY));
    }

    public void print_registers()
    {
        TestRomLoad romLoader = GameObject.FindObjectOfType<TestRomLoad>();
        string text = ("A: "  + AF.a.ToString("X2") + " F: " + AF.f.ToString("X2")
        + " \nB: "  + BC.b.ToString("X2") + " C: " + BC.c.ToString("X2")
        + " \nD: "  + DE.d.ToString("X2") + " E: " + DE.e.ToString("X2")
        + " \nH: "  + HL.h.ToString("X2") + " L: " + HL.l.ToString("X2")
        + " \nSP: "  + sp.ToString("X2")
        + " \nPC: "  + pc.ToString("X2") + "\n"
        + "Status: " + "Running: " + romLoader.status.isRunning + " Paused: "+ romLoader.status.isPaused + " Do Step: " + romLoader.status.doStep + "\n LY: " + romLoader.ppu.scanline + " \nZ: " + is_flag_set((byte)RegisterFlags.FLAG_ZERO) + " \nN: " + is_flag_set((byte)RegisterFlags.FLAG_SUBTRACT) + " \nH: " + is_flag_set((byte)RegisterFlags.FLAG_HALF_CARRY) + " \nC: " + is_flag_set((byte)RegisterFlags.FLAG_CARRY));

        debugLabel.GetComponent<TextMeshProUGUI>().text = text;
    }
}
