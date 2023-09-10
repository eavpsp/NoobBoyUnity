using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum ButtonFlags
{
    JOYPAD_A = (1 << 0),
    JOYPAD_B = (1 << 1),
    JOYPAD_SELECT = (1 << 2),
    JOYPAD_START = (1 << 3),

    JOYPAD_RIGHT = (1 << 4),
    JOYPAD_LEFT = (1 << 5),
    JOYPAD_UP = (1 << 6),
    JOYPAD_DOWN = (1 << 7),
};

public class Joypad
{
    public Status._Status status;
    public MMU mmu;
    public Interrupt interrupts;

    public int joypad_state;
    public int joypad_cycles = 0;

    public Joypad(Status._Status status, Interrupt interrupts, MMU mmu)
    {
        this.interrupts = interrupts;
        this.mmu = mmu;
        this.status = status;
    }
    void key_press(ButtonFlags key)
    {
        this.joypad_state = mmu.joypad & ~(0xFF & (int)key);
        Debug.Log("Joypad State: " + joypad_state);
    }

    void key_release(ButtonFlags key)
    {
        this.joypad_state = mmu.joypad | (0xFF & (int)key);
    }
    void update_joypad_memory()
    {
        if (this.joypad_state == 1)
        {
            mmu.joypad = (byte)this.joypad_state;
            this.joypad_state = 0;
            interrupts.set_interrupt_flag((int)InterruptFlags.INTERRUPT_JOYPAD);
        }
    }

    public void check(int last_instr_cycles)
    {
        joypad_cycles += last_instr_cycles;
        if (!status.isPaused)
        {
            if (joypad_cycles < 65536)
                return;
            joypad_cycles -= 65536;
        }

        joypad_cycles += last_instr_cycles;

            if (!status.isPaused)
            {
                if (joypad_cycles < 65536)
                    return;

            joypad_cycles -= 65536;
            }

        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
                key_press(ButtonFlags.JOYPAD_RIGHT);
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                key_press(ButtonFlags.JOYPAD_LEFT);
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                key_press(ButtonFlags.JOYPAD_UP);
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                key_press(ButtonFlags.JOYPAD_DOWN);
            else if (Input.GetKeyDown(KeyCode.Z))
                key_press(ButtonFlags.JOYPAD_A);
            else if (Input.GetKeyDown(KeyCode.X))
                key_press(ButtonFlags.JOYPAD_B);
            else if (Input.GetKeyDown(KeyCode.Space))
                key_press(ButtonFlags.JOYPAD_START);
            else if (Input.GetKeyDown(KeyCode.Return))
                key_press(ButtonFlags.JOYPAD_SELECT);
            else if (Input.GetKeyDown(KeyCode.Escape))
                status.isRunning = false;
        }

        if (Input.anyKey)
        {
            if (Input.GetKeyUp(KeyCode.RightArrow))
                key_press(ButtonFlags.JOYPAD_RIGHT);
            else if (Input.GetKeyUp(KeyCode.LeftArrow))
                key_press(ButtonFlags.JOYPAD_LEFT);
            else if (Input.GetKeyUp(KeyCode.UpArrow))
                key_press(ButtonFlags.JOYPAD_UP);
            else if (Input.GetKeyUp(KeyCode.DownArrow))
                key_press(ButtonFlags.JOYPAD_DOWN);
            else if (Input.GetKeyUp(KeyCode.Z))
                key_press(ButtonFlags.JOYPAD_A);
            else if (Input.GetKeyUp(KeyCode.X))
                key_press(ButtonFlags.JOYPAD_B);
            else if (Input.GetKeyUp(KeyCode.Space))
                key_press(ButtonFlags.JOYPAD_START);
            else if (Input.GetKeyUp(KeyCode.Return))
                key_press(ButtonFlags.JOYPAD_SELECT);
        }

            update_joypad_memory();
        }
}
