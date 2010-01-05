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
Imports System.IO
Imports System.Exception
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Modules.Blog.Business
Imports DotNetNuke.Modules.Blog.File
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Services.Exceptions
Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Common.Globals



Partial Class EditEntry
 Inherits BlogModuleBase

#Region "Public Properties"
 Public ReadOnly Property FilePath() As String
  Get
   Return Me.PortalSettings.HomeDirectory & Me.ModuleConfiguration.FriendlyName & "/"
  End Get
 End Property
#End Region

#Region "Private member"
 Private m_oEntryController As New EntryController
 Private m_oEntry As EntryInfo
 Private m_oBlogController As New BlogController
 Private m_oParentBlog As BlogInfo
 Private m_oBlog As BlogInfo
 'Private m_oBlogSettings As Hashtable
 Private m_oTagController As New TagController
 Private m_oTags As ArrayList
 Private m_oCategoryController As New CategoryController
 Private m_oCats As List(Of Business.CategoryInfo)
 Private m_oEntryCats As List(Of Business.CategoryInfo)
#End Region

#Region "Controls"
 Protected WithEvents txtDescription As DotNetNuke.UI.UserControls.TextEditor
 Protected WithEvents chkDisplaySocialBookmarks As System.Web.UI.WebControls.CheckBox
 Protected WithEvents teBlogEntry As DotNetNuke.UI.UserControls.TextEditor
#End Region

#Region "Event Handlers"

 Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
  Try

   valDescription.Enabled = BlogSettings.EntryDescriptionRequired
   txtDescriptionOptional.Visible = Not valDescription.Enabled
   Me.pnlUploads.Visible = BlogSettings.EnableUploadOption

   'Localize the file attachments datagrid
   Localization.LocalizeDataGrid(dgLinkedFiles, LocalResourceFile)

   If Not Page.IsPostBack Then
    DotNetNuke.UI.Utilities.ClientAPI.AddButtonConfirm(cmdDelete, Localization.GetString("DeleteItem"))
    cboChildBlogs.DataSource = m_oBlogController.ListBlogs(Me.PortalId, m_oParentBlog.BlogID, True)
    cboChildBlogs.DataBind()
    cboChildBlogs.Items.Insert(0, New ListItem(m_oParentBlog.Title, m_oParentBlog.BlogID.ToString()))
    If Not cboChildBlogs.Items.FindByValue(m_oBlog.BlogID.ToString()) Is Nothing Then
     cboChildBlogs.Items.FindByValue(m_oBlog.BlogID.ToString()).Selected = True
    End If
    ' change in 3.1.23 not display the trackback url field if auto trackback is enabled
    Me.lblTrackbackUrl.Visible = Not m_oParentBlog.AutoTrackback
    Me.txtTrackBackUrl.Visible = Not m_oParentBlog.AutoTrackback
    ' end change in 3.1.23
    'DR-19/04/2009-BLG-9760
    If m_oBlog IsNot Nothing AndAlso m_oBlog.EnableTwitterIntegration Then
     chkDoNotTweet.Visible = True
    Else
     chkDoNotTweet.Visible = False
    End If

    Dim cc As New CategoryController
    cc.PopulateTree(treeCategories, PortalId, 0)

    If Not m_oEntry Is Nothing Then
     'Load data
     txtEntryDate.Text = Utility.FormatDate(m_oEntry.AddedDate, m_oBlog.Culture, m_oBlog.DateFormat, m_oBlog.TimeZone)
     txtTitle.Text = m_oEntry.Title
     If BlogSettings.AllowSummaryHtml Then
      txtDescription.Text = m_oEntry.Description
     Else
      txtDescription.Visible = False
      txtDescriptionText.Visible = True
      txtDescriptionText.Text = m_oEntry.Description
     End If
     teBlogEntry.Text = Server.HtmlDecode(m_oEntry.Entry)
     'DR-04/16/2009-BLG-9657
     lblPublished.Visible = Not m_oEntry.Published
     chkAllowComments.Checked = m_oEntry.AllowComments
     chkDisplayCopyright.Checked = m_oEntry.DisplayCopyright
     If chkDisplayCopyright.Checked Then
      pnlCopyright.Visible = True
      txtCopyright.Text = m_oEntry.Copyright
     End If
     'DR-05/28/2009-BLG-9556
     txtEntryDate.ReadOnly = m_oEntry.Published
     cmdDelete.Visible = True
     cboChildBlogs.Enabled = True
     Me.dgLinkedFiles.DataSource = FileController.getFileList(Me.FilePath, m_oEntry)
     Me.dgLinkedFiles.DataBind()
     chkDoNotTweet.Checked = True

     'RR-09/01/2009-BLG-XXXX
     tbTags.Text = m_oTagController.GetTagsByEntry(m_oEntry.EntryID)

     'm_oCats = m_oCategoryController.ListCategoriesSorted(m_oBlog.PortalID)
     m_oEntryCats = m_oCategoryController.ListCatsByEntry(m_oEntry.EntryID)

     'ddlCategory.DataSource = m_oCats
     'ddlCategory.DataBind()
     '' Need to localize next line
     'ddlCategory.Items.Insert(0, New ListItem(" - Uncategorized - ", "-1"))
     '' RR-data structures are in place for multi-category, but UI is single at this point
     'If Not m_oEntryCats Is Nothing Then
     ' If Not m_oEntryCats.Count = 0 Then
     '  If Not ddlCategory.Items.FindByValue(m_oEntryCats(0).CatID.ToString) Is Nothing Then
     '   ddlCategory.Items.FindByValue(m_oEntryCats(0).CatID.ToString).Selected = True
     '  End If
     ' End If
     'End If

     For Each c As CategoryInfo In m_oEntryCats
      treeCategories.FindNodeByKey(c.CatID.ToString).Selected = True
     Next

    Else
     'DR-04/16/2009-BLG-9657
     lblPublished.Visible = True
     chkAllowComments.Checked = m_oBlog.AllowComments
     txtEntryDate.Text = Utility.FormatDate(Date.UtcNow, m_oBlog.Culture, m_oBlog.DateFormat, m_oBlog.TimeZone)
     chkDoNotTweet.Checked = False
    End If

    If Not Request.UrlReferrer Is Nothing Then
     ViewState("URLReferrer") = Request.UrlReferrer.ToString
    End If

    ' stuff for tag suggestions
    Dim TagController As New TagController
    Dim TagAList As ArrayList
    TagAList = TagController.ListTags(PortalId)
    Dim TagSList(TagAList.Count) As String
    For i As Integer = 0 To TagAList.Count - 1
     TagSList(i) = Replace(CType(TagAList(i), TagInfo).Tag, " ", "_")
    Next

    Dim TagString As String
    TagString = "'" + String.Join("','", TagSList) + "'"

    If Not Page.ClientScript.IsClientScriptBlockRegistered("JQUERY") Then
     Dim jQueryScript As String = "<script src=""" & ModulePath & "js/jquery.min.js"" type=""text/javascript""></script>"
     ' Dim jQueryScript As String = "<script src=""http://ajax.googleapis.com/ajax/libs/jquery/1.3.2/jquery.min.js"" type=""text/javascript""></script>"
     Page.ClientScript.RegisterClientScriptBlock(Me.GetType(), "JQUERY", jQueryScript)
    End If


    If Not Page.ClientScript.IsClientScriptBlockRegistered("TAG") Then
     Dim TagScript As String = "<script src=""" & ModulePath & "js/tag.js"" type=""text/javascript""></script>"
     Page.ClientScript.RegisterClientScriptBlock(Me.GetType(), "TAG", TagScript)
    End If


    If Not Page.ClientScript.IsClientScriptBlockRegistered("TAGSUGGEST") Then
     Dim TagSuggestScript As String = "<script type=""text/javascript"">jQuery(function(){setGlobalTags([" + TagString + "]);jQuery('#" + tbTags.ClientID + "').tagSuggest({separator:','});});</script>"
     Page.ClientScript.RegisterClientScriptBlock(Me.GetType(), "TAGSUGGEST", TagSuggestScript)
    End If

   End If

  Catch exc As Exception
   ProcessModuleLoadException(Me, exc)
  End Try
 End Sub

 Private Sub cmdPublish_Click(ByVal sender As Object, ByVal e As EventArgs) Handles cmdPublish.Click
  updateEntry(True, True)
 End Sub

 Private Sub cmdDraft_Click(ByVal sender As Object, ByVal e As EventArgs) Handles cmdDraft.Click
  'DR-04/16/2009-BLG-9657
  updateEntry(False, False)
 End Sub

 Private Sub cmdCancel_Click(ByVal sender As Object, ByVal e As EventArgs) Handles cmdCancel.Click
  Try
   'Antonio Chagoury
   'BLG-5813
   'Response.Redirect(CType(ViewState("URLReferrer"), String), True)
   If Not Request.QueryString("BlogId") Is Nothing Then
    Response.Redirect(Utility.AddTOQueryString(NavigateURL(), "BlogID", Request.QueryString("BlogId").ToString()), True)
   ElseIf Not Request.QueryString("EntryId") Is Nothing Then
    Response.Redirect(Utility.AddTOQueryString(NavigateURL(), "EntryId", Request.QueryString("EntryId").ToString()), True)
   Else
    Response.Redirect(NavigateURL(), True)
   End If

  Catch exc As Exception    'Module failed to load
   ProcessModuleLoadException(Me, exc)
  End Try
 End Sub

 Private Sub cmdDelete_Click(ByVal sender As Object, ByVal e As EventArgs) Handles cmdDelete.Click
  Try
   If Not m_oEntry Is Nothing Then
    DeleteAllFiles()
    m_oEntryController.DeleteEntry(m_oEntry.EntryID)
    Response.Redirect(Utility.AddTOQueryString(NavigateURL(), "BlogID", m_oEntry.BlogID.ToString()), True)
   Else
    Response.Redirect(NavigateURL(), True)
   End If
  Catch exc As Exception    'Module failed to load
   ProcessModuleLoadException(Me, exc)
  End Try
 End Sub

 Private Sub valEntry_ServerValidate(ByVal source As Object, ByVal args As System.Web.UI.WebControls.ServerValidateEventArgs) Handles valEntry.ServerValidate
  args.IsValid = teBlogEntry.Text.Length > 0
 End Sub

 Private Sub valEntryDateData_ServerValidate(ByVal source As System.Object, ByVal args As System.Web.UI.WebControls.ServerValidateEventArgs) Handles valEntryDateData.ServerValidate
  args.IsValid = Utility.IsValidDate(txtEntryDate.Text, m_oBlog.Culture)
 End Sub

 Private Sub chkDisplayCopyright_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDisplayCopyright.CheckedChanged
  pnlCopyright.Visible = chkDisplayCopyright.Checked
  If pnlCopyright.Visible Then
   txtCopyright.Text = CreateCopyRight()
  End If
 End Sub

 Private Sub dgLinkedFiles_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.DataGridItemEventArgs) Handles dgLinkedFiles.ItemDataBound
  Dim lnkDeleteFile As ImageButton
  If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.EditItem Or e.Item.ItemType = ListItemType.AlternatingItem Or e.Item.ItemType = ListItemType.SelectedItem Then
   lnkDeleteFile = CType(e.Item.FindControl("lnkDeleteFile"), System.Web.UI.WebControls.ImageButton)
   If Not lnkDeleteFile Is Nothing Then
    If dgLinkedFiles.EditItemIndex = -1 Then
     lnkDeleteFile.Attributes.Add("onclick", "return confirm('" & String.Format(Localization.GetString("msgEnsureDeleteFile", Me.LocalResourceFile), lnkDeleteFile.CommandName) & "');")
     lnkDeleteFile.ImageUrl = Me.ModulePath & "Images/delete_file.gif"
    End If
   End If
  End If
 End Sub

 Protected Sub lnkDeleteFile_Command(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.CommandEventArgs)
  DeleteFile(e.CommandName)
 End Sub

 Private Sub btnUploadPicture_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnUploadPicture.Click
  Dim bResponse As Boolean = False
  Dim strImage As String
  If m_oEntry Is Nothing Then
   'DW - 06/06/08 - Doesn't seem to be needed.
   'txtDescription.Text = txtDescription.Text & "UPLOADTEMPLATE"
   teBlogEntry.Text = teBlogEntry.Text & "UPLOADTEMPLATE"
   Me.valDescription.IsValid = True
   Me.valEntry.IsValid = True
   updateEntry(False, False)
   'DW - 06/06/08 - Doesn't seem to be needed.
   'txtDescription.Text = txtDescription.Text.Replace("UPLOADTEMPLATE", String.Empty)
   bResponse = True
  End If
  If Me.picFilename.PostedFile.FileName <> "" Then
   Dim maxImageWidth As Integer
   maxImageWidth = BlogSettings.MaxImageWidth
   strImage = uploadImage(Me.FilePath, m_oEntry, 0)
   If strImage <> "" Then
    Dim pHeight As Integer
    Dim pWidth As Integer
    Dim fullImage As System.Drawing.Image
    fullImage = System.Drawing.Image.FromFile(strImage)
    If fullImage.Width > maxImageWidth Then
     pWidth = maxImageWidth
     pHeight = CType((fullImage.Height / (fullImage.Width / maxImageWidth)), Integer)
    Else
     pWidth = fullImage.Width
     pHeight = fullImage.Height
    End If
    strImage = FileController.getVirtualFileName(FilePath, strImage)
    teBlogEntry.Text = teBlogEntry.Text & "<img src='" & strImage & " 'Alt='" & Me.txtAltText.Text & "' Width='" & pWidth.ToString & "' Height='" & pHeight.ToString & "' >"
   End If
  End If
  If bResponse Then
   teBlogEntry.Text = teBlogEntry.Text.Replace("UPLOADTEMPLATE", "")
   updateEntry(False, False)

   If Not m_oEntry Is Nothing Then
    Response.Redirect(EditUrl("EntryID", m_oEntry.EntryID.ToString(), "Edit_Entry"))
   End If

  Else
   Me.dgLinkedFiles.DataSource = FileController.getFileList(Me.FilePath, m_oEntry)
   Me.dgLinkedFiles.DataBind()
  End If
 End Sub

 Private Sub btnUploadAttachment_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnUploadAttachment.Click
  Dim bResponse As Boolean = False
  Dim strAttachment As String
  Dim strDescription As String
  If m_oEntry Is Nothing Then
   'DW - 06/06/08 - Doesn't seem to be needed.
   'txtDescription.Text = txtDescription.Text & "UPLOADTEMPLATE"
   teBlogEntry.Text = teBlogEntry.Text & "UPLOADTEMPLATE"
   Me.valDescription.IsValid = True
   Me.valEntry.IsValid = True
   updateEntry(False, False)
   'DW - 06/06/08 - Doesn't seem to be needed.
   'txtDescription.Text = txtDescription.Text.Replace("UPLOADTEMPLATE", "")
   bResponse = True
  End If
  If Me.attFilename.PostedFile.FileName <> "" Then
   If Me.txtAttachmentDescription.Text = "" Then
    strDescription = System.IO.Path.GetFileName(attFilename.PostedFile.FileName)
   Else
    strDescription = Me.txtAttachmentDescription.Text
   End If
   ' content type will not work with FTB / may be in a later version
   'strExtension = Me.attFilename.PostedFile.ContentType
   'strExtension = Path.GetExtension(Me.attFilename.PostedFile.FileName).Replace(".", "")
   'strExtension = strExtension.ToLower()
   strAttachment = uploadFile(Me.FilePath, m_oEntry, 1)
   If strAttachment <> "" Then
    '    Dim strContentType As String
    '    Select Case strExtension
    '        Case "txt" : strContentType = "text/plain"
    '        Case "htm", "html" : strContentType = "text/html"
    '        Case "rtf" : strContentType = "text/richtext"
    '        Case "jpg", "jpeg" : strContentType = "image/jpeg"
    '        Case "gif" : strContentType = "image/gif"
    '        Case "bmp" : strContentType = "image/bmp"
    '        Case "mpg", "mpeg" : strContentType = "video/mpeg"
    '        Case "avi" : strContentType = "video/avi"
    '        Case "pdf" : strContentType = "application/pdf"
    '        Case "doc", "dot" : strContentType = "application/msword"
    '        Case "csv", "xls", "xlt" : strContentType = "application/x-msexcel"
    '        Case Else : strContentType = "application/octet-stream"
    '    End Select

    strAttachment = FileController.getVirtualFileName(FilePath, strAttachment)
    teBlogEntry.Text = teBlogEntry.Text & "<a href='" & strAttachment & "'>" & strDescription & "</a>"
   End If

  End If
  If bResponse Then
   teBlogEntry.Text = teBlogEntry.Text.Replace("UPLOADTEMPLATE", "")
   updateEntry(False, False)

   If Not m_oEntry Is Nothing Then
    Response.Redirect(EditUrl("EntryID", m_oEntry.EntryID.ToString(), "Edit_Entry"))
   End If

  Else
   Me.dgLinkedFiles.DataSource = FileController.getFileList(Me.FilePath, m_oEntry)
   Me.dgLinkedFiles.DataBind()
  End If

 End Sub

#End Region

#Region "Private Methods"

 Private Sub updateEntry(ByVal publish As Boolean, ByVal redirect As Boolean)
  Try
   If Page.IsValid = True Then
    If m_oEntry Is Nothing Then
     m_oEntry = New EntryInfo
     m_oEntry = CType(CBO.InitializeObject(m_oEntry, GetType(EntryInfo)), EntryInfo)
    End If
    With m_oEntry
     'bind text values to object
     .BlogID = m_oBlog.BlogID
     .Title = txtTitle.Text
     'DR-04/16/2009-BLG-9658
     Dim descriptionText As String = ""
     If BlogSettings.AllowSummaryHtml Then
      descriptionText = Trim(txtDescription.Text)
     Else
      descriptionText = (New DotNetNuke.Security.PortalSecurity).InputFilter(Trim(txtDescriptionText.Text), Security.PortalSecurity.FilterFlag.NoMarkup)
     End If
     If (descriptionText.Length = 0) OrElse (descriptionText = "&lt;p&gt;&amp;#160;&lt;/p&gt;") Then
      .Description = Nothing
     Else
      .Description = descriptionText
     End If

     .Entry = teBlogEntry.Text
     'DR-04/16/2009-BLG-9657
     .Published = publish
     .AllowComments = chkAllowComments.Checked
     .DisplayCopyright = chkDisplayCopyright.Checked
     .Copyright = txtCopyright.Text

     If Not cboChildBlogs.SelectedItem Is Nothing Then
      If CType(cboChildBlogs.SelectedItem.Value, Double) > 0 Then
       .BlogID = CType(cboChildBlogs.SelectedItem.Value, Integer)
      End If
     End If
     'DR-04/19/2009-BLG-9760
     If publish AndAlso (Not chkDoNotTweet.Checked) Then m_oBlog.EnableTwitterIntegration = True
     If Null.IsNull(m_oEntry.EntryID) Then
      .AddedDate = Utility.ToLocalTime(Utility.ParseDate(txtEntryDate.Text, m_oBlog.Culture), m_oBlog.TimeZone)
      .EntryID = m_oEntryController.AddEntry(m_oEntry)
      .PermaLink = Utility.GenerateEntryLink(PortalId, .EntryID, Me.TabId, .Title)
      m_oEntryController.UpdateEntry(m_oEntry, m_oBlog)
     Else
      .AddedDate = Utility.ParseDate(txtEntryDate.Text, m_oBlog.Culture)
      .PermaLink = Utility.GenerateEntryLink(PortalId, .EntryID, Me.TabId, .Title)
      m_oEntryController.UpdateEntry(m_oEntry)
     End If

     m_oTagController.UpdateTagsByEntry(m_oEntry.EntryID, tbTags.Text)
     'm_oCategoryController.UpdateCategoriesByEntry(m_oEntry.EntryID, CInt(ddlCategory.SelectedValue))
     Dim selectedCategories As New List(Of Integer)
     For Each n As UI.WebControls.TreeNode In treeCategories.SelectedTreeNodes
      selectedCategories.Add(CInt(n.Key))
     Next
     m_oCategoryController.UpdateCategoriesByEntry(m_oEntry.EntryID, selectedCategories)

     If txtTrackBackUrl.Text <> "" Then
      PingBackService.SendTrackBack(txtTrackBackUrl.Text, m_oEntry, m_oBlog.Title)
     End If
     If m_oBlog.AutoTrackback Then
      Utility.AutoTrackback(m_oEntry, m_oBlog.Title)
     End If
     If redirect Then
      Response.Redirect(NavigateURL(Me.TabId, "", "BlogID=" & .BlogID.ToString()), True)
     Else
      lblPublished.Visible = Not publish
      'DR-05/28/2009-BLG-9556
      txtEntryDate.ReadOnly = publish
     End If
    End With


   End If
  Catch exc As Exception    'Module failed to load
   ProcessModuleLoadException(Me, exc)
  End Try

 End Sub

 Private Function CreateCopyRight() As String
  Return Localization.GetString("msgCopyright", LocalResourceFile) & Date.UtcNow.Year & " " & m_oBlog.UserFullName
 End Function

#End Region

#Region " Web Form Designer Generated Code "

 'This call is required by the Web Form Designer.
 <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

 End Sub

 'NOTE: The following placeholder declaration is required by the Web Form Designer.
 'Do not delete or move it.
 Private designerPlaceholderDeclaration As System.Object

 Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
  'CODEGEN: This method call is required by the Web Form Designer
  'Do not modify it using the code editor.
  InitializeComponent()

  Try
   If Not (Request.Params("EntryID") Is Nothing) Then
    m_oEntry = m_oEntryController.GetEntry(Int32.Parse(Request.Params("EntryID")), PortalId)
    m_oBlog = m_oBlogController.GetBlog(m_oEntry.BlogID)
   ElseIf Not (Request.Params("BlogID") Is Nothing) Then
    m_oBlog = m_oBlogController.GetBlog(Int32.Parse(Request.Params("BlogID")))
   Else
    m_oBlog = m_oBlogController.GetBlogByUserID(Me.PortalId, Me.UserId)
   End If
  Catch
  End Try

  If m_oBlog Is Nothing Then
   Response.Redirect(Request.UrlReferrer.ToString, True)
  ElseIf Not m_oEntry Is Nothing Then
   Me.ModuleConfiguration.ModuleTitle = Localization.GetString("msgEditBlogEntry", LocalResourceFile)
  Else
   Me.ModuleConfiguration.ModuleTitle = Localization.GetString("msgAddBlogEntry", LocalResourceFile)
  End If
  If Not Utility.HasBlogPermission(Me.UserId, m_oBlog.UserID, Me.ModuleId) Then
   Response.Redirect(NavigateURL())
  End If

  If m_oBlog.ParentBlogID > -1 Then
   m_oParentBlog = m_oBlogController.GetBlog(m_oBlog.ParentBlogID)
  Else
   m_oParentBlog = m_oBlog
  End If

 End Sub

#End Region

#Region "Upload Feature Methods"

 Private Function uploadImage(ByVal pFilePath As String, ByVal pEntry As EntryInfo, ByVal FileType As Integer) As String
  uploadImage = UploadFiles(Me.PortalId, FileController.getEntryDir(pFilePath, pEntry), Me.picFilename.PostedFile, FileType)
 End Function

 Private Function uploadFile(ByVal pFilePath As String, ByVal pEntry As EntryInfo, ByVal FileType As Integer) As String
  uploadFile = UploadFiles(Me.PortalId, FileController.getEntryDir(pFilePath, pEntry), Me.attFilename.PostedFile, FileType)
 End Function

 Private Function UploadFiles(ByVal PortalId As Integer, ByVal EntryPath As String, ByVal objFile As HttpPostedFile, ByVal FileType As Integer) As String

  Dim objPortalController As New PortalController
  Dim strMessage As String = ""
  Dim strFileName As String = ""
  Dim strExtension As String = ""


  If objFile.FileName <> "" Then
   strFileName = EntryPath & Path.GetFileName(objFile.FileName)
   strExtension = Replace(Path.GetExtension(strFileName), ".", "")
  End If

  ' FileType 0 == Picture
  If FileType = 0 Then
   If strExtension <> "" Then
    If strExtension.ToLower() <> "jpg" And strExtension.ToLower() <> "gif" And strExtension.ToLower() <> "png" Then
     Me.valUpload.ErrorMessage = Localization.GetString("valUpload.ErrorMessage", LocalResourceFile)
     Me.valUpload.ErrorMessage = Me.valUpload.ErrorMessage.Replace("[FILENAME]", objFile.FileName)
     Me.valUpload.IsValid = False
     'The File [FILENAME] Is A Restricted File Type for Images. Valid File Types Include JPG, GIF and PNG<br>
     Return ""
    End If
   End If
  End If

  If ((((objPortalController.GetPortalSpaceUsedBytes(PortalId) + objFile.ContentLength) / 1000000) <= Me.PortalSettings.HostSpace) Or Me.PortalSettings.HostSpace = 0) Or (Me.PortalSettings.ActiveTab.ParentId = Me.PortalSettings.SuperTabId) Then

   If (InStr(1, "," & Me.PortalSettings.HostSettings("FileExtensions").ToString.ToUpper, "," & strExtension.ToUpper) <> 0) Or Me.PortalSettings.ActiveTab.ParentId = Me.PortalSettings.SuperTabId Then
    Try
     If strFileName <> "" Then
      If System.IO.File.Exists(strFileName) Then
       System.IO.File.SetAttributes(strFileName, FileAttributes.Normal)
       System.IO.File.Delete(strFileName)
      End If
      ' DW - 04/16/2008 - Check to make sure the directory exists.
      FileController.createFileDirectory(strFileName)
      objFile.SaveAs(strFileName)
      strMessage = strFileName
      Me.valUpload.IsValid = True
     End If

    Catch ex As Exception
     ProcessModuleLoadException(String.Format(Localization.GetString("SaveFileError"), strFileName), Me, ex, True)
    End Try
   Else
    Me.valUpload.ErrorMessage = String.Format(Localization.GetString("RestrictedFileType"), strFileName, Replace(Me.PortalSettings.HostSettings("FileExtensions").ToString, ",", ", *."))
    Me.valUpload.IsValid = False
   End If

  Else
   Me.valUpload.ErrorMessage = String.Format(Localization.GetString("DiskSpaceExceeded"), strFileName)
   Me.valUpload.IsValid = False
  End If
  Return strMessage
 End Function

 Private Sub DeleteFile(ByVal fileName As String)
  Try
   Dim _filePath As String = FileController.getEntryDir(Me.FilePath, m_oEntry)
   Dim _fullName As String = _filePath & fileName
   System.IO.File.Delete(_fullName)
   Me.dgLinkedFiles.DataSource = FileController.getFileList(Me.FilePath, m_oEntry)
   Me.dgLinkedFiles.DataBind()
  Catch

  End Try
 End Sub

 Private Sub DeleteAllFiles()
  Try
   System.IO.Directory.Delete(FileController.getEntryDir(Me.FilePath, m_oEntry), True)
  Catch

  End Try

 End Sub

#End Region

End Class

