﻿using System.Collections;
using System.IO;
using System.Diagnostics;
using Affdex;
using System.Text;
using System.Linq;
using System;

public class FileOutput
{
	private StreamWriter writer;
	private Stopwatch stopwatch;
	private bool started = false;

	public FileOutput(string filename)
	{
		writer = new StreamWriter (File.OpenWrite(filename));
		writer.AutoFlush = true;
		stopwatch = new Stopwatch ();
		stopwatch.Start ();
	}

	public void Close()
	{
		writer.Close ();
		stopwatch.Stop ();
	}

	public void LogEvent(string msg)
	{
		writer.WriteLine(string.Format("%s\t%s", stopwatch.Elapsed.Seconds, msg));
	}

	public void LogFace(Face face)
	{
		if (!started) {
			WriteHeader (face);
			started = true;
		}

		var sb = new StringBuilder ();
		sb.Append (stopwatch.Elapsed.TotalSeconds).Append ("\t");

		foreach (var emotion in face.Emotions) {
			sb.Append (emotion.Value.ToString("F3")).Append ("\t");
		}
		foreach (var expression in face.Expressions) {
			sb.Append (expression.Value.ToString("F3")).Append ("\t");
		}
		//UnityEngine.Debug.Log (sb.ToString());

		writer.WriteLine(sb.ToString());
	}

	public void WriteHeader(Face face)
	{
		var sb = new StringBuilder ();
		sb.Append ("Time\t");

		foreach (var emotion in face.Emotions) {
			sb.Append (emotion.Key).Append ("\t");
		}
		foreach (var expression in face.Expressions) {
			sb.Append (expression.Key).Append ("\t");
		}

		writer.WriteLine (sb.ToString());
	}
}

