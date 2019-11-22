
using System;
using UnityEngine;

public class BitBuffer {
	private long _bits = 0;
	private int _currentBitCount = 0;
	public int _length = 0;
	public int _seek = 0;
	private readonly byte [] _buffer;

	public void PutBit(bool value)
	{
		PutBits(value ? 1 : 0, 1);
	}

	private void AddByte()
	{
		while (_currentBitCount >= 8)
		{
			_buffer[_seek++] = (byte) _bits;
			_length++;
			_currentBitCount -= 8;
			_bits >>= 8;
		}
	}

	public void PutBits(long value, int bitCount)
	{
		long mask = 0;
		for(int i=0; i<bitCount;i++)
		{
			mask <<= 1;
			mask++;
		}

		long val = value & mask;
		_bits |= (val << _currentBitCount);
		_currentBitCount += bitCount;
		AddByte();
	}

	public void PutInt(int value, int min, int max)
	{
		int range = max - min;
		PutBits(value-min,(int)Math.Ceiling(Math.Log(range+1,2)));

	}

	public int GetInt(int min, int max)
	{
		int range = max - min;
		return (int) GetBits((int) Math.Log(range, 2) + 1) + min;
	}

	public void PutFloat(float value, float min, float max, float step)
	{
		int val = (int) ((value - min) / step);
		int maxi = (int) ((max - min) / step);
		PutInt(val,0,maxi);
	}

	public float GetFloat(float min, float max, float step)
	{
		int maxi = (int) ((max - min) / step);
		int val = GetInt(0, maxi);
		return val * step + min;
	}

	private void Flush()
	{
		_length = 0;
		_seek = 0;
		_bits = 0;
		_currentBitCount = 0;

	}

	public bool GetBit()
	{
		return GetBits(1) == 1;

	}

	public long GetBits(int bitcount)
	{
		long mask = 0;
		for (int i = 0; i < bitcount; i++)
		{
			mask <<= 1;
			mask++;
		}
		GetByte(bitcount);
		long ret = _bits & mask;
		_currentBitCount -= bitcount;
		_bits >>= bitcount;
		return ret;

	}

	private void GetByte(int bitcount)
	{
		while (_currentBitCount < bitcount)
		{
			_bits |= (uint) _buffer[_seek] << _currentBitCount;
			_seek++;
			_currentBitCount += 8;
		}
	}
	public BitBuffer()
	{
		_buffer = new byte[512];
	}

	public BitBuffer(byte[] payload)
	{
		_buffer = new byte[512];
		for (int i=0; i < payload.Length; i++)
		{
			_buffer[i] = payload[i];
			_length++;
		}
	}

	public byte[] GetPayload()
	{
		byte[] ret = new byte[_length+1];
		for (int i = 0; i < _length; i++)
		{
			ret[i] = _buffer[i];
		}
		
		ret[_length] = (byte)_bits;
		Flush();
		return ret;
	}
}
