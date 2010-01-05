﻿'
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

''' <summary>
''' This class abstracts all settings for the module and makes sure they're (a) defaulted and (b) hard typed 
''' throughout the application.
''' </summary>
''' <remarks></remarks>
''' <history>
'''		[pdonker]	12/30/2009	created
''' </history>
Public Class TagViewSettings

#Region " Private Members "
 Private _allSettings As Hashtable
 Private _tabModuleId As Integer = -1
 Private _TagDisplayMode As String = "List"
#End Region

#Region " Constructors "
 Public Sub New(ByVal TabModuleId As Integer)

  _tabModuleId = TabModuleId
  _allSettings = (New DotNetNuke.Entities.Modules.ModuleController).GetTabModuleSettings(_tabModuleId)
  Globals.ReadValue(_allSettings, "TagDisplayMode", TagDisplayMode)

 End Sub

 Public Shared Function GetTagViewSettings(ByVal TabModuleId As Integer) As TagViewSettings
  Dim CacheKey As String = "TagViewSettings" & TabModuleId.ToString
  Dim bs As TagViewSettings = CType(DotNetNuke.Common.Utilities.DataCache.GetCache(CacheKey), TagViewSettings)
  If bs Is Nothing Then
   bs = New TagViewSettings(TabModuleId)
   DotNetNuke.Common.Utilities.DataCache.SetCache(CacheKey, bs)
  End If
  Return bs
 End Function
#End Region

#Region " Public Members "
 Public Overridable Sub UpdateSettings()

  Dim objModules As New DotNetNuke.Entities.Modules.ModuleController
  With objModules
   .UpdateTabModuleSetting(_tabModuleId, "TagDisplayMode", TagDisplayMode)
  End With

 End Sub
#End Region

#Region " Properties "
 Public Property TagDisplayMode() As String
  Get
   Return _TagDisplayMode
  End Get
  Set(ByVal Value As String)
   _TagDisplayMode = Value
  End Set
 End Property
#End Region

End Class
