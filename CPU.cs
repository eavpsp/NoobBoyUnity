using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPU
{
    public Instructions instructions;
    public MMU memory;
    public Interrupt interrupts;
    public Registers registers;

    public CPU(Registers registers, Interrupt interrupts, MMU memory)
    {
        this.memory = memory;
        this.interrupts = interrupts;
        this.registers = registers;
        this.instructions = new Instructions(this.registers, this.interrupts, this.memory);
        
    }

    public void no_bootrom_init()
    {
        registers.AF.a = 0x01;
        registers.AF.f = 0xb0;
        registers.BC.b = 0x00;
        registers.BC.c = 0x13;
        registers.DE.d = 0x00;
        registers.DE.e = 0xd8;
        registers.HL.h = 0x01;
        registers.HL.l = 0x4d;

        registers.sp = 0xfffe;
        registers.pc = 0x0100;

        registers.set_flags((byte)RegisterFlags.FLAG_ZERO | (byte)RegisterFlags.FLAG_HALF_CARRY | (byte)RegisterFlags.FLAG_CARRY, true);
        registers.set_flags((byte)RegisterFlags.FLAG_SUBTRACT, false);

        memory.romDisabled = true;
        memory.memory[0xFF0F] = 0xE1;
        memory.write_byte(0xFF41, 0x80);
        memory.write_byte(0xFF40, 0x91);
  

        memory._timer.div = 0xD3;
        memory._timer.tima = 0x00;
        memory._timer.tma = 0x00;
        memory._timer.tac = 0xF8;
        
    }

    public void step()
    {
       
        if (memory.is_halted)
        {
            memory._clock.t_instr = 4;
            Debug.Log("Memory Halted");
            return;
        }

        byte instruction = this.memory.read_byte(registers.pc);

        if (!memory.trigger_halt_bug)
            registers.pc++;

        memory.trigger_halt_bug = false;
        instructions.execute(instruction);
        
    }
}
