using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum InterruptFlags { INTERRUPT_VBLANK = (1 << 0), INTERRUPT_LCD = (1 << 1), INTERRUPT_TIMER = (1 << 2), INTERRUPT_SERIAL = (1 << 3), INTERRUPT_JOYPAD = (1 << 4) };

public class Interrupt
{
    public Registers registers;
    public MMU mmu;
    public bool IME = true;

    public Interrupt(Registers reg, MMU _mmu)
    {
        this.mmu = _mmu;
        this.registers = reg;
    }
    public void set_master_flag(bool state)
    {
        this.IME = state;
    }

    public bool is_master_enabled()
    {
        return this.IME;
    }
    public void set_interrupt_flag(byte flag)
    {
        byte IF_value = this.mmu.read_byte(0xFF0F);
        IF_value |= flag;
        this.mmu.write_byte(0xFF0F, IF_value);
    }
    public void unset_interrupt_flag(byte flag)
    {
        byte IF_value = mmu.read_byte(0xFF0F);
        IF_value &= (byte)~flag;
        mmu.write_byte(0xFF0F, IF_value);
    }
    public bool is_interrupt_enabled(byte flag)
    {
         return (mmu.read_byte(0xFFFF) & flag) != 0;
    }
    public bool is_interrupt_flag_set(byte flag)
    {
        return (mmu.read_byte(0xFF0F) & flag) !=0;
    }
    public bool check()
    {
        if ((mmu.read_byte(0xFFFF) & mmu.read_byte(0xFF0F) & 0x0F) != 0)
        {
            mmu.is_halted = false;
        }

        if (!is_master_enabled())
        {
            return false;
        }

        // Check if VBLANK
        if (is_interrupt_enabled((byte)InterruptFlags.INTERRUPT_VBLANK) && is_interrupt_flag_set((byte)InterruptFlags.INTERRUPT_VBLANK))
        {
            trigger_interrupt(InterruptFlags.INTERRUPT_VBLANK, 0x40);
            Debug.Log("VBLank Interrupt");

            return true;
        }

        // Check if LCD
        if (is_interrupt_enabled((byte)InterruptFlags.INTERRUPT_LCD) && is_interrupt_flag_set((byte)InterruptFlags.INTERRUPT_LCD))
        {
            trigger_interrupt(InterruptFlags.INTERRUPT_LCD, 0x48);
            Debug.Log("LCD Interrupt");

            return true;
        }

        // Check if TIMER
        if (is_interrupt_enabled((byte)InterruptFlags.INTERRUPT_TIMER) && is_interrupt_flag_set((byte)InterruptFlags.INTERRUPT_TIMER))
        {
            trigger_interrupt(InterruptFlags.INTERRUPT_TIMER, 0x50);
            Debug.Log("Timer Interrupt");
            return true;
        }

        // Check if JOYPAD
        if (is_interrupt_enabled((byte)InterruptFlags.INTERRUPT_JOYPAD) && is_interrupt_flag_set((byte)InterruptFlags.INTERRUPT_JOYPAD))
        {
            trigger_interrupt(InterruptFlags.INTERRUPT_JOYPAD, 0x60);
            Debug.Log("Joypad Interrupt");

            return true;
        }

        return false;
    }
    public void trigger_interrupt(InterruptFlags interrupt, byte jump_pc)
    {
        this.mmu.write_short_stack(ref registers.sp, this.registers.pc);
        this.registers.pc = jump_pc;
        this.set_master_flag(false);
        this.unset_interrupt_flag((byte)interrupt);
        mmu.is_halted = false;

        mmu._clock.t_instr = 20;
    }
}
