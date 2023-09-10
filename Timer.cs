using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer 
{
    public Registers registers;
    public MMU mmu;
    public Interrupt interrupts;

    public int gameboy_ticks = 4 * 1024 * 1024; // 4194304

    public int div = 0;
    public int tac = 0;
    public int tima = 0;

   public Timer(MMU mmu, Interrupt interrupts)
    {
        this.mmu = mmu;
        this.interrupts = interrupts;
    }

    public void inc()
    {
        
        mmu._clock.t += mmu._clock.t_instr;
        mmu._clock.t %= gameboy_ticks;

        div += mmu._clock.t_instr;

        while (div >= 256)
        {
            div -= 256;
            mmu._timer.div++;
        }

        check();
        
    }

    void check()
    {
        if ((mmu._timer.tac & 0x04) != 0)
        {
            tima += mmu._clock.t_instr;

            int threshold = 0;
            switch (mmu._timer.tac & 0x03)
            {
                case 0:
                    threshold = 1024;
                    break;
                case 1:
                    threshold = 16;
                    break;
                case 2:
                    threshold = 64;
                    break;
                case 3:
                    threshold = 256;
                    break;
            }
            while (tima >= threshold)
            {
                tima -= threshold;
                if (mmu._timer.tima == 0xFF)
                {
                    mmu._timer.tima = mmu.read_byte(0xFF06);
                    interrupts.set_interrupt_flag((byte)InterruptFlags.INTERRUPT_TIMER);
                }
                else
                {
                    mmu._timer.tima++;
                }
            }
        }
    }
}
