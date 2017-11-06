package com.coolape.lang;

import java.io.EOFException;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.text.DecimalFormat;

public class NumEx {
	public static final boolean stringToBool(String s, boolean v) {
		try {
			return Boolean.parseBoolean(s);
		} catch (Exception e) {
			return v;
		}
	}

	public static final boolean stringToBool(String s) {
		return stringToBool(s, false);
	}

	public static final byte stringToByte(String s, byte v) {
		try {
			return Byte.parseByte(s);
		} catch (Exception e) {
			return v;
		}
	}

	public static final byte stringToByte(String s) {
		return stringToByte(s, (byte) 0);
	}

	public static final byte[] stringToByte(String[] v) {
		byte[] r = new byte[v.length];
		int n = 0;
		for (String s : v) {
			r[n] = stringToByte(s);
			n++;
		}
		return r;
	}

	public static final short stringToShort(String s, short v) {
		try {
			return Short.parseShort(s);
		} catch (Exception e) {
			return v;
		}
	}

	public static final short stringToShort(String s) {
		return stringToShort(s, (short) 0);
	}

	public static final short[] stringToShort(String[] v) {
		short[] r = new short[v.length];
		int n = 0;
		for (String s : v) {
			r[n] = stringToShort(s);
			n++;
		}
		return r;
	}

	public static final int stringToInt(String s, int v) {
		try {
			return Integer.parseInt(s);
		} catch (Exception e) {
			return v;
		}
	}

	public static final int stringToInt(String s) {
		return stringToInt(s, 0);
	}

	public static int[] stringToInt(String[] v) {
		int[] r = new int[v.length];
		int n = 0;
		for (String s : v) {
			r[n] = stringToInt(s);
			n++;
		}
		return r;
	}

	public static final long stringToLong(String s, long v) {
		try {
			return Long.parseLong(s);
		} catch (Exception e) {
			return v;
		}
	}

	public static final long stringToLong(String s) {
		return stringToLong(s, 0);
	}

	public static final long[] stringToLong(String[] v) {
		long[] r = new long[v.length];
		int n = 0;
		for (String s : v) {
			r[n] = stringToLong(s);
			n++;
		}
		return r;
	}

	public static final float stringToFloat(String s, float v) {
		try {
			return Float.parseFloat(s);
		} catch (Exception e) {
			return v;
		}
	}

	public static final float stringToFloat(String s) {
		return stringToFloat(s, (float) 0.0);
	}

	public static final float[] stringToFloat(String[] v) {
		float[] r = new float[v.length];
		int n = 0;
		for (String s : v) {
			r[n] = stringToFloat(s);
			n++;
		}
		return r;
	}

	public static final double stringToDouble(String s, double v) {
		try {
			return Double.parseDouble(s);
		} catch (Exception e) {
			return v;
		}
	}

	public static final double stringToDouble(String s) {
		return stringToDouble(s, 0.0);
	}

	public static final double[] stringToDouble(String[] v) {
		double[] r = new double[v.length];
		int n = 0;
		for (String s : v) {
			r[n] = stringToDouble(s);
			n++;
		}
		return r;
	}

	private static final int read(InputStream input) throws IOException {
		int value = input.read();
		if (-1 == value)
			throw new EOFException("Unexpected EOF reached");
		return value;
	}

	public static final void writeShort(byte[] data, int offset, short value) {
		data[offset + 0] = (byte) ((value >> 8) & 0xff);
		data[offset + 1] = (byte) ((value >> 0) & 0xff);
	}

	public static final short readShort(byte[] data, int offset) {
		return (short) (((data[offset + 0] & 0xff) << 8) + ((data[offset + 1] & 0xff) << 0));
	}

	public static final int readUnsignedShort(byte[] data, int offset) {
		return (((data[offset + 0] & 0xff) << 8) + ((data[offset + 1] & 0xff) << 0));
	}

	public static final void writeUnsignedShort(byte[] data, int offset, int value) {
		data[offset + 0] = (byte) ((value >> 24) & 0xff);
		data[offset + 1] = (byte) ((value >> 16) & 0xff);
	}

	public static final void writeInt(byte[] data, int offset, int value) {
		data[offset + 0] = (byte) ((value >> 24) & 0xff);
		data[offset + 1] = (byte) ((value >> 16) & 0xff);
		data[offset + 2] = (byte) ((value >> 8) & 0xff);
		data[offset + 3] = (byte) ((value >> 0) & 0xff);
	}

	public static final int readInt(byte[] data, int offset) {
		return (((data[offset + 0] & 0xff) << 24) + ((data[offset + 1] & 0xff) << 16) + ((data[offset + 2] & 0xff) << 8) + ((data[offset + 3] & 0xff) << 0));
	}

	public static final void writeLong(byte[] data, int offset, long value) {
		data[offset + 0] = (byte) ((value >> 56) & 0xff);
		data[offset + 1] = (byte) ((value >> 48) & 0xff);
		data[offset + 2] = (byte) ((value >> 40) & 0xff);
		data[offset + 3] = (byte) ((value >> 32) & 0xff);
		data[offset + 4] = (byte) ((value >> 24) & 0xff);
		data[offset + 5] = (byte) ((value >> 16) & 0xff);
		data[offset + 6] = (byte) ((value >> 8) & 0xff);
		data[offset + 7] = (byte) ((value >> 0) & 0xff);
	}

	public static final long readLong(byte[] data, int offset) {
		long high = ((data[offset + 0] & 0xff) << 24) + ((data[offset + 1] & 0xff) << 16) + ((data[offset + 2] & 0xff) << 8) + ((data[offset + 3] & 0xff) << 0);
		long low = ((data[offset + 4] & 0xff) << 24) + ((data[offset + 5] & 0xff) << 16) + ((data[offset + 6] & 0xff) << 8) + ((data[offset + 7] & 0xff) << 0);
		return (high << 32) + (0xffffffffL & low);
	}

	public static final void writeFloat(byte[] data, int offset, float value) {
		writeInt(data, offset, Float.floatToIntBits(value));
	}

	public static final float readFloat(byte[] data, int offset) {
		return Float.intBitsToFloat(readInt(data, offset));
	}

	public static final void writeDouble(byte[] data, int offset, double value) {
		writeLong(data, offset, Double.doubleToLongBits(value));
	}

	public static final double readDouble(byte[] data, int offset) {
		return Double.longBitsToDouble(readLong(data, offset));
	}

	public static final void writeShort(OutputStream output, short value) throws IOException {
		output.write((byte) ((value >> 8) & 0xff));
		output.write((byte) ((value >> 0) & 0xff));
	}

	public static final short readShort(InputStream input) throws IOException {
		return (short) (((read(input) & 0xff) << 8) + ((read(input) & 0xff) << 0));
	}

	public static final void writeInt(OutputStream output, int value) throws IOException {
		output.write((byte) ((value >> 24) & 0xff));
		output.write((byte) ((value >> 16) & 0xff));
		output.write((byte) ((value >> 8) & 0xff));
		output.write((byte) ((value >> 0) & 0xff));
	}

	public static final int readInt(InputStream input) throws IOException {
		int value1 = read(input);
		int value2 = read(input);
		int value3 = read(input);
		int value4 = read(input);

		return ((value1 & 0xff) << 24) + ((value2 & 0xff) << 16) + ((value3 & 0xff) << 8) + ((value4 & 0xff) << 0);
	}

	public static final void writeLong(OutputStream output, long value) throws IOException {
		output.write((byte) ((value >> 56) & 0xff));
		output.write((byte) ((value >> 48) & 0xff));
		output.write((byte) ((value >> 40) & 0xff));
		output.write((byte) ((value >> 32) & 0xff));
		output.write((byte) ((value >> 24) & 0xff));
		output.write((byte) ((value >> 16) & 0xff));
		output.write((byte) ((value >> 8) & 0xff));
		output.write((byte) ((value >> 0) & 0xff));
	}

	public static final long readLong(InputStream input) throws IOException {
		byte[] bytes = new byte[8];
		for (int i = 0; i < 8; i++) {
			bytes[i] = (byte) read(input);
		}
		return readLong(bytes, 0);
	}

	public static final void writeFloat(OutputStream output, float value) throws IOException {
		writeInt(output, Float.floatToIntBits(value));
	}

	public static final float readFloat(InputStream input) throws IOException {
		return Float.intBitsToFloat(readInt(input));
	}

	public static final void writeDouble(OutputStream output, double value) throws IOException {
		writeLong(output, Double.doubleToLongBits(value));
	}

	public static final double readDouble(InputStream input) throws IOException {
		return Double.longBitsToDouble(readLong(input));
	}

	public static final int readUnsignedShort(InputStream input) throws IOException {
		int value1 = read(input);
		int value2 = read(input);

		return (((value1 & 0xff) << 8) + ((value2 & 0xff) << 0));
	}

	public static final void writeUnsignedShort(OutputStream os, int value) throws IOException {
		byte[] data = new byte[2];
		data[0] = (byte) ((value >> 8) & 0xff);
		data[1] = (byte) ((value >> 0) & 0xff);
		os.write(data);
	}

	public static final boolean isByte(String s) {
		try {
			Byte.parseByte(s);
			return true;
		} catch (Exception e) {
			return false;
		}
	}

	public static final boolean isShort(String s) {
		try {
			Short.parseShort(s);
			return true;
		} catch (Exception e) {
			return false;
		}
	}

	public static final boolean isInt(String s) {
		try {
			Integer.parseInt(s);
			return true;
		} catch (Exception e) {
			return false;
		}
	}

	public static final boolean isLong(String s) {
		try {
			Long.parseLong(s);
			return true;
		} catch (Exception e) {
			return false;
		}
	}

	public static final boolean isFloat(String s) {
		try {
			Float.parseFloat(s);
			return true;
		} catch (Exception e) {
			return false;
		}
	}

	public static final boolean isDouble(String s) {
		try {
			Double.parseDouble(s);
			return true;
		} catch (Exception e) {
			return false;
		}
	}

	static final DecimalFormat _decimalFormat = new DecimalFormat(".00");

	public static String formatDouble(double s) {
		return _decimalFormat.format(s);
	}

	public static final String fix6Int(int v) {
		return String.format("%06d", v);
	}

	public static final String fix3Int(int v) {
		return String.format("%03d", v);
	}

	public static final short swapShort(short value) {
		return (short) ((((value >> 0) & 0xff) << 8) + (((value >> 8) & 0xff) << 0));
	}

	public static final int swapInteger(int value) {
		return (((value >> 0) & 0xff) << 24) + (((value >> 8) & 0xff) << 16) + (((value >> 16) & 0xff) << 8) + (((value >> 24) & 0xff) << 0);
	}

	public static final long swapLong(long value) {
		return (((value >> 0) & 0xff) << 56) + (((value >> 8) & 0xff) << 48) + (((value >> 16) & 0xff) << 40) + (((value >> 24) & 0xff) << 32) + (((value >> 32) & 0xff) << 24)
				+ (((value >> 40) & 0xff) << 16) + (((value >> 48) & 0xff) << 8) + (((value >> 56) & 0xff) << 0);
	}

	public static final float swapFloat(float value) {
		return Float.intBitsToFloat(swapInteger(Float.floatToIntBits(value)));
	}

	public static final double swapDouble(double value) {
		return Double.longBitsToDouble(swapLong(Double.doubleToLongBits(value)));
	}

	public static final void writeSwappedShort(byte[] data, int offset, short value) {
		data[offset + 0] = (byte) ((value >> 0) & 0xff);
		data[offset + 1] = (byte) ((value >> 8) & 0xff);
	}

	public static final short readSwappedShort(byte[] data, int offset) {
		return (short) (((data[offset + 0] & 0xff) << 0) + ((data[offset + 1] & 0xff) << 8));
	}

	public static final int readSwappedUnsignedShort(byte[] data, int offset) {
		return (((data[offset + 0] & 0xff) << 0) + ((data[offset + 1] & 0xff) << 8));
	}

	public static final void writeSwappedInteger(byte[] data, int offset, int value) {
		data[offset + 0] = (byte) ((value >> 0) & 0xff);
		data[offset + 1] = (byte) ((value >> 8) & 0xff);
		data[offset + 2] = (byte) ((value >> 16) & 0xff);
		data[offset + 3] = (byte) ((value >> 24) & 0xff);
	}

	public static final int readSwappedInteger(byte[] data, int offset) {
		return (((data[offset + 0] & 0xff) << 0) + ((data[offset + 1] & 0xff) << 8) + ((data[offset + 2] & 0xff) << 16) + ((data[offset + 3] & 0xff) << 24));
	}

	public static final long readSwappedUnsignedInteger(byte[] data, int offset) {
		long low = (((data[offset + 0] & 0xff) << 0) + ((data[offset + 1] & 0xff) << 8) + ((data[offset + 2] & 0xff) << 16));

		long high = data[offset + 3] & 0xff;

		return (high << 24) + (0xffffffffL & low);
	}

	public static final void writeSwappedLong(byte[] data, int offset, long value) {
		data[offset + 0] = (byte) ((value >> 0) & 0xff);
		data[offset + 1] = (byte) ((value >> 8) & 0xff);
		data[offset + 2] = (byte) ((value >> 16) & 0xff);
		data[offset + 3] = (byte) ((value >> 24) & 0xff);
		data[offset + 4] = (byte) ((value >> 32) & 0xff);
		data[offset + 5] = (byte) ((value >> 40) & 0xff);
		data[offset + 6] = (byte) ((value >> 48) & 0xff);
		data[offset + 7] = (byte) ((value >> 56) & 0xff);
	}

	public static final long readSwappedLong(byte[] data, int offset) {
		long low = ((data[offset + 0] & 0xff) << 0) + ((data[offset + 1] & 0xff) << 8) + ((data[offset + 2] & 0xff) << 16) + ((data[offset + 3] & 0xff) << 24);
		long high = ((data[offset + 4] & 0xff) << 0) + ((data[offset + 5] & 0xff) << 8) + ((data[offset + 6] & 0xff) << 16) + ((data[offset + 7] & 0xff) << 24);
		return (high << 32) + (0xffffffffL & low);
	}

	public static final void writeSwappedFloat(byte[] data, int offset, float value) {
		writeSwappedInteger(data, offset, Float.floatToIntBits(value));
	}

	public static final float readSwappedFloat(byte[] data, int offset) {
		return Float.intBitsToFloat(readSwappedInteger(data, offset));
	}

	public static final void writeSwappedDouble(byte[] data, int offset, double value) {
		writeSwappedLong(data, offset, Double.doubleToLongBits(value));
	}

	public static final double readSwappedDouble(byte[] data, int offset) {
		return Double.longBitsToDouble(readSwappedLong(data, offset));
	}

	public static final void writeSwappedShort(OutputStream output, short value) throws IOException {
		output.write((byte) ((value >> 0) & 0xff));
		output.write((byte) ((value >> 8) & 0xff));
	}

	public static final short readSwappedShort(InputStream input) throws IOException {
		return (short) (((read(input) & 0xff) << 0) + ((read(input) & 0xff) << 8));
	}

	public static final int readSwappedUnsignedShort(InputStream input) throws IOException {
		int value1 = read(input);
		int value2 = read(input);

		return (((value1 & 0xff) << 0) + ((value2 & 0xff) << 8));
	}

	public static final void writeSwappedInteger(OutputStream output, int value) throws IOException {
		output.write((byte) ((value >> 0) & 0xff));
		output.write((byte) ((value >> 8) & 0xff));
		output.write((byte) ((value >> 16) & 0xff));
		output.write((byte) ((value >> 24) & 0xff));
	}

	public static final int readSwappedInteger(InputStream input) throws IOException {
		int value1 = read(input);
		int value2 = read(input);
		int value3 = read(input);
		int value4 = read(input);

		return ((value1 & 0xff) << 0) + ((value2 & 0xff) << 8) + ((value3 & 0xff) << 16) + ((value4 & 0xff) << 24);
	}

	public static final long readSwappedUnsignedInteger(InputStream input) throws IOException {
		int value1 = read(input);
		int value2 = read(input);
		int value3 = read(input);
		int value4 = read(input);

		long low = (((value1 & 0xff) << 0) + ((value2 & 0xff) << 8) + ((value3 & 0xff) << 16));

		long high = value4 & 0xff;

		return (high << 24) + (0xffffffffL & low);
	}

	public static final void writeSwappedLong(OutputStream output, long value) throws IOException {
		output.write((byte) ((value >> 0) & 0xff));
		output.write((byte) ((value >> 8) & 0xff));
		output.write((byte) ((value >> 16) & 0xff));
		output.write((byte) ((value >> 24) & 0xff));
		output.write((byte) ((value >> 32) & 0xff));
		output.write((byte) ((value >> 40) & 0xff));
		output.write((byte) ((value >> 48) & 0xff));
		output.write((byte) ((value >> 56) & 0xff));
	}

	public static final long readSwappedLong(InputStream input) throws IOException {
		byte[] bytes = new byte[8];
		for (int i = 0; i < 8; i++) {
			bytes[i] = (byte) read(input);
		}
		return readSwappedLong(bytes, 0);
	}

	public static final void writeSwappedFloat(OutputStream output, float value) throws IOException {
		writeSwappedInteger(output, Float.floatToIntBits(value));
	}

	public static final float readSwappedFloat(InputStream input) throws IOException {
		return Float.intBitsToFloat(readSwappedInteger(input));
	}

	public static final void writeSwappedDouble(OutputStream output, double value) throws IOException {
		writeSwappedLong(output, Double.doubleToLongBits(value));
	}

	public static final double readSwappedDouble(InputStream input) throws IOException {
		return Double.longBitsToDouble(readSwappedLong(input));
	}

}
