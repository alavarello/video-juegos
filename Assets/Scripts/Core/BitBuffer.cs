
using System;
using UnityEngine;

public class BitBuffer {
	private long _bits = 0;
	public int _length = 0;
	private int _bitCount = 0;
	private readonly byte [] _buffer;
	public int _searches = 0;


	public byte[] GetByteArray()
	{
		byte[] ret = new byte[_length+1];
		for (int i = 0; i < _length; i++)
		{
			ret[i] = _buffer[i];
		}
		
		ret[_length] = (byte)_bits;
		Clear();
		return ret;
	}


	private void AddByte()
	{
		while (_bitCount >= 8)
		{
			_buffer[_searches++] = (byte) _bits;
			_length++;
			_bitCount -= 8;
			_bits >>= 8;
		}
	}

	
	public void InsertBit(bool value)
	{
		InsertBits(value ? 1 : 0, 1);
	}

	public void InsertInt(int value, int min, int max)
	{
		int range = max - min;
		InsertBits(value-min,(int)Math.Ceiling(Math.Log(range+1,2)));
	}
	
	
	public void InsertBits(long value, int bitCount)
	{
		long bits = 0;
		for(int i=0; i<bitCount;i++)
		{
			bits <<= 1;
			bits++;
		}

		long val = value & bits;
		_bits |= (val << _bitCount);
		_bitCount += bitCount;
		AddByte();
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
		InsertInt(val,0,maxi);
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
		_bitCount -= bitcount;
		_bits >>= bitcount;
		return ret;
	}
	
	
	public float GetFloat(float min, float max, float step)
	{
		int maxi = (int) ((max - min) / step);
		int val = GetInt(0, maxi);
		return val * step + min;
	}

	private void GetByte(int bitcount)
	{
		while (_bitCount < bitcount)
		{
			_bits |= (uint) _buffer[_searches] << _bitCount;
			_searches++;
			_bitCount += 8;
		}
	}
	public BitBuffer()
	{
		_buffer = new byte[1024];
	}

	public BitBuffer(byte[] byteArray)
	{
		_buffer = new byte[1024];
		for (int i=0; i < byteArray.Length; i++)
		{
			_buffer[i] = byteArray[i];
			_length++;
		}
	}

	private void Clear()
	{
		_length = 0;
		_searches = 0;
		_bits = 0;
		_bitCount = 0;

	}

}
