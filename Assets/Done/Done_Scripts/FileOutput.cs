using System.Collections;
using System.IO;
using System.Diagnostics;
using Affdex;
using System.Text;
using System.Linq;

public class FileOutput
{
	private StreamWriter writer;
	private Stopwatch stopwatch;

	public FileOutput(string filename)
	{
		writer = new StreamWriter (filename);
		WriteHeader ();
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
		var sb = new StringBuilder ();
		sb.Append (stopwatch.Elapsed.Seconds).Append ("\t");

		foreach (var emotion in face.Emotions) {
			sb.Append (emotion.Value).Append ("\t");
		}
		foreach (var expression in face.Expressions) {
			sb.Append (expression).Append ("\t");
		}

		writer.WriteLine(sb.ToString());
	}

	public void WriteHeader()
	{
		var face = new Face (new FaceData ());
		var sb = new StringBuilder ();
		sb.Append ("Time");

		foreach (var emotion in face.Emotions) {
			sb.Append (emotion).Append ("\t");
		}
		foreach (var expression in face.Expressions) {
			sb.Append (expression).Append ("\t");
		}

		writer.WriteLine (sb.ToString());
	}
}

