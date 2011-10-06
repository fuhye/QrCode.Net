﻿using System;
using System.Text;
using System.Collections.Generic;
using Mode = com.google.zxing.qrcode.decoder.Mode;

namespace Gma.QrCodeNet.Encoding.Tests.DataEncodation
{
	/// <summary>
	/// Description of KanjiEncoderTestCaseFactory.
	/// </summary>
	public class KanjiEncoderTestCaseFactory : EncoderTestCaseFactoryBase
	{
		protected override string CsvFileName { get { return "KanjiEncoderTestDataSet.csv"; } }
		
		protected override string GenerateRandomInputString(int inputSize, Random randomizer)
        {
			return GenerateRandomKanjiString(inputSize, randomizer);
        }
		
		protected override IEnumerable<bool> EncodeUsingReferenceImplementation(string content, int version)
        {
            return EncodeUsingReferenceImplementation(content, version, Mode.KANJI);
        }
		
		
		
		/*
         * Main part of Kanji is from two separate group.
		 *Each group separate to several table. Table is control by most significant byte. 
	     *First group: 0x81?? to 0x9F?? Second group: 0xE0?? to 0xEB??
		 *Each table's value is inside least significant byte range. 
		 *Least significant byte boundary: Lower 0x40, Upper 0xFC
		*/
		/// <summary>
		/// Generate Random Kanji string
		/// </summary>
		/// <remarks>
		/// Generate random char according to Shift_JIS table layout. 
		/// </remarks>
		/// <see cref="URL: http://interscript.sourceforge.net/interscript/doc/en_shiftjis_0003.html"></see>
		protected string GenerateRandomKanjiString(int inputSize, Random randomizer)
        {
        	StringBuilder result = new StringBuilder(inputSize);
        	Decoder shiftJisDecoder = System.Text.Encoding.GetEncoding("Shift_JIS").GetDecoder();
        	for(int i = 0; i < inputSize; i++)
        	{
        		
        		int RandomShiftJISChar = RandomGenerateKanjiChar(randomizer);
        		
        		byte[] bytes = ConvertShortToByte(RandomShiftJISChar);
        		int charLength = shiftJisDecoder.GetCharCount(bytes, 0, bytes.Length);
        		
        		if(charLength == 1)
        		{
        			char[] kanjiChar = new char[shiftJisDecoder.GetCharCount(bytes, 0, bytes.Length)];
        			shiftJisDecoder.GetChars(bytes, 0, bytes.Length, kanjiChar, 0);
        			result.Append(kanjiChar[0]);
        		}
        		else
        			throw new ArgumentOutOfRangeException("Random Kanji Char decode fail. Decode result contain more than one char or zero char");
        	}
        	return result.ToString();
        }

        
        private const int FIRST_LOWER_BOUNDARY = 0x889F;
        private const int FIRST_UPPER_BOUNDARY = 0x9FFC;
        
        private const int SECOND_LOWER_BOUNDARY = 0xE040;
		private const int SECOND_UPPER_BOUNDARY = 0xEAA4;
		
        /// <summary>
        /// Random generate a Kanji char
        /// </summary>
        /// <remarks>Kanji Shift JIS char separate to two group</remarks>
        private int RandomGenerateKanjiChar(Random randomizer)
        {
        	return randomizer.Next(0, 2) == 0 ? RandomGenerateKanjiCharFromTable(FIRST_LOWER_BOUNDARY, FIRST_UPPER_BOUNDARY, randomizer)
        		: RandomGenerateKanjiCharFromTable(SECOND_LOWER_BOUNDARY, SECOND_UPPER_BOUNDARY, randomizer);
        }
        
        /// <summary>
        /// Generate Kanji char for specific Group
        /// </summary>
        /// <remarks>
        /// Each group separate to several table. Each table's least significant char will start from 0x40 to 0xFC
        /// </remarks>
        private int RandomGenerateKanjiCharFromTable(int lowerBoundary, int upperBoundary, Random randomizer)
        {
        	int RandomShiftJISChar = 0;
        	do
       		{
        		RandomShiftJISChar = randomizer.Next(lowerBoundary, upperBoundary + 1);
        	} while(isCharOutsideTableRange(RandomShiftJISChar));
        	return RandomShiftJISChar;
        }
        
        /// <summary>
        /// Convert short to byte array.
        /// </summary>
        /// <remarks>
        /// Bitconverter's return value is reversed order. Need to be correct. 
        /// </remarks>
        private byte[] ConvertShortToByte(int value)
        {
        	byte[] converterBytes = BitConverter.GetBytes((short)value);
        	return new byte[]{converterBytes[1], converterBytes[0]};
        }
        
        
        /// <summary>
		/// URL: http://interscript.sourceforge.net/interscript/doc/en_shiftjis_0003.html
		/// Each table start from 40
		/// </summary>
		private const int TABLE_LOWER_BOUNDARY = 0x40;
		/// <summary>
		/// URL: http://interscript.sourceforge.net/interscript/doc/en_shiftjis_0003.html
		/// Each table end with FC
		/// </summary>
		private const int TABLE_UPPER_BOUNDARY = 0xFC;
		
        /// <summary>
        /// Shift JIS is separate to several table. Each table's Least significant byte start from 0x40 to 0xFC
        /// </summary>
        /// <returns>True if outside table range. Else return false</returns>
        private bool isCharOutsideTableRange(int RandomChar)
        {
        	int LeastSignificantByte = RandomChar & 0xFF;
        	return (LeastSignificantByte < TABLE_LOWER_BOUNDARY || LeastSignificantByte > TABLE_UPPER_BOUNDARY);
        }
		
	}
}
