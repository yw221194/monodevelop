// 
// CodeFormatter.cs
//  
// Author:
//       Mike Krüger <mkrueger@novell.com>
// 
// Copyright (c) 2009 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects.Policies;
using MonoDevelop.Core.Text;
using MonoDevelop.Core;

namespace MonoDevelop.Ide.CodeFormatting
{
	public abstract class AbstractCodeFormatter
	{
		protected abstract ITextSource FormatImplementation (PolicyContainer policyParent, string mimeType, ITextSource input, int startOffset, int endOffset);

		public ITextSource Format (PolicyContainer policyParent, string mimeType, ITextSource input, int fromOffset, int toOffset)
		{
			if (fromOffset < 0 || fromOffset > input.Length)
				throw new ArgumentOutOfRangeException (nameof (fromOffset), "should be >= 0 && < " + input.Length + " was:" + fromOffset);
			if (toOffset < 0 || toOffset > input.Length)
				throw new ArgumentOutOfRangeException (nameof (toOffset), "should be >= 0 && < " + input.Length + " was:" + toOffset);
			try {
				return FormatImplementation (policyParent ?? PolicyService.DefaultPolicies, mimeType, input, fromOffset, toOffset);
			} catch (Exception e) {
				LoggingService.LogError ("Error while formatting text.", e);
			}
			return input.CreateSnapshot (fromOffset, toOffset - fromOffset);
		}

		public ITextSource Format (PolicyContainer policyParent, string mimeType, ITextSource input)
		{
			if (input == null)
				throw new ArgumentNullException (nameof (input));
			return Format (policyParent ?? PolicyService.DefaultPolicies, mimeType, input, 0, input.Length);
		}

		public string FormatText (PolicyContainer policyParent, string mimeType, string input, int fromOffset, int toOffset)
		{
			if (input == null)
				throw new ArgumentNullException (nameof (input));
			if (mimeType == null)
				throw new ArgumentNullException (nameof (mimeType));
			return FormatImplementation (policyParent, mimeType, new StringTextSource (input), fromOffset, toOffset).Text;
		}

		public string FormatText (PolicyContainer policyParent, string mimeType, string input)
		{
			if (input == null)
				throw new ArgumentNullException (nameof (input));
			return FormatText (policyParent ?? PolicyService.DefaultPolicies, mimeType, input, 0, input.Length);
		}

		public virtual bool SupportsOnTheFlyFormatting { get { return false; } }

		protected virtual void OnTheFlyFormatImplementation (TextEditor editor, DocumentContext context, int startOffset, int endOffset)
		{
			throw new NotSupportedException ("On the fly formatting not supported");
		}

		public virtual void OnTheFlyFormat (TextEditor editor, DocumentContext context, int startOffset, int endOffset)
		{
			if (editor == null)
				throw new ArgumentNullException (nameof (editor));
			if (context == null)
				throw new ArgumentNullException (nameof (context));

			if (startOffset < 0 || startOffset > editor.Length)
				throw new ArgumentOutOfRangeException (nameof (startOffset), "should be >= 0 && < " + editor.Length + " was:" + startOffset);
			if (endOffset < 0 || endOffset > editor.Length)
				throw new ArgumentOutOfRangeException (nameof (endOffset), "should be >= 0 && < " + editor.Length + " was:" + endOffset);

			OnTheFlyFormatImplementation (editor, context, startOffset, endOffset);
		}

		public void OnTheFlyFormat (Document ideDocument, int startOffset, int endOffset)
		{
			OnTheFlyFormat (ideDocument.Editor, ideDocument, startOffset, endOffset);
		}

		public virtual bool SupportsCorrectingIndent { get { return false; } }

		protected virtual void CorrectIndentingImplementation (PolicyContainer policyParent, TextEditor editor, int line)
		{
			throw new NotSupportedException ("Indent correction not supported");
		}

		public void CorrectIndenting (PolicyContainer policyParent, TextEditor editor, int line)
		{
			if (policyParent == null)
				throw new ArgumentNullException (nameof (policyParent));
			if (editor == null)
				throw new ArgumentNullException (nameof (editor));
			if (line < 0 || line >= editor.LineCount)
				throw new ArgumentOutOfRangeException (nameof (line), "should be >= 0 && < " + editor.LineCount + " was:" + line);
			CorrectIndentingImplementation (policyParent, editor, line);
		}
	}
}