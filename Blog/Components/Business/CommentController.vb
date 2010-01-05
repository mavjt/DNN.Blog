'
' DotNetNuke -  http://www.dotnetnuke.com
' Copyright (c) 2002-2010
' by Perpetual Motion Interactive Systems Inc. ( http://www.perpetualmotion.ca )
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'-------------------------------------------------------------------------

Imports System
Imports DotNetNuke.Modules.Blog.Data
Imports DotNetNuke.Common.Utilities

Namespace Business

 Public Class CommentController

  Public Function GetComment(ByVal commentID As Integer) As CommentInfo

   Return CType(CBO.FillObject(DataProvider.Instance().GetComment(commentID), GetType(CommentInfo)), CommentInfo)

  End Function

  Public Function ListComments(ByVal EntryID As Integer, ByVal Approved As Boolean) As ArrayList

   Return CBO.FillCollection(DataProvider.Instance().ListComments(EntryID, Approved), GetType(CommentInfo))

  End Function

  Public Function AddComment(ByVal objComment As CommentInfo) As Integer

   Return CType(DataProvider.Instance().AddComment(objComment.EntryID, objComment.UserID, objComment.Title, objComment.Comment, objComment.Author, objComment.Approved, objComment.Website, objComment.Email), Integer)

  End Function

  Public Sub UpdateComment(ByVal objComment As CommentInfo)

   DataProvider.Instance().UpdateComment(objComment.CommentID, objComment.EntryID, objComment.UserID, objComment.Title, objComment.Comment, objComment.Author, objComment.Approved, objComment.Website, objComment.Email)

  End Sub

  Public Sub DeleteComment(ByVal commentID As Integer)

   DataProvider.Instance().DeleteComment(commentID)

  End Sub

  Public Sub DeleteAllUnapproved(ByVal EntryID As Integer)

   DataProvider.Instance().DeleteAllUnapproved(EntryID)

  End Sub


 End Class

End Namespace