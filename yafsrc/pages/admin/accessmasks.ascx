<%@ Control language="c#" Codebehind="accessmasks.ascx.cs" AutoEventWireup="false" Inherits="yaf.pages.admin.accessmasks" %>
<%@ Register TagPrefix="yaf" Namespace="yaf.controls" Assembly="yaf" %>

<yaf:adminmenu runat="server">

<table class=content cellSpacing=1 cellPadding=0 width="100%">
<tr>
	<td class="header1" colspan="12">Access Masks</td>
</tr>
<tr class="header2">
	<td>Name</td>
	<td align="center">Read</td>
	<td align="center">Post</td>
	<td align="center">Reply</td>
	<td align="center">Priority</td>
	<td align="center">Poll</td>
	<td align="center">Vote</td>
	<td align="center">Moderator</td>
	<td align="center">Edit</td>
	<td align="center">Delete</td>
	<td align="center">Upload</td>
	<td>&nbsp;</td>
</tr>

<asp:repeater id="List" runat="server">
<ItemTemplate>
		<tr class="post">
			<td>
				<%# DataBinder.Eval(Container.DataItem, "Name") %>
			</td>
			<td align="center"><%# DataBinder.Eval(Container.DataItem, "ReadAccess") %></td>
			<td align="center"><%# DataBinder.Eval(Container.DataItem, "PostAccess") %></td>
			<td align="center"><%# DataBinder.Eval(Container.DataItem, "ReplyAccess") %></td>
			<td align="center"><%# DataBinder.Eval(Container.DataItem, "PriorityAccess") %></td>
			<td align="center"><%# DataBinder.Eval(Container.DataItem, "PollAccess") %></td>
			<td align="center"><%# DataBinder.Eval(Container.DataItem, "VoteAccess") %></td>
			<td align="center"><%# DataBinder.Eval(Container.DataItem, "ModeratorAccess") %></td>
			<td align="center"><%# DataBinder.Eval(Container.DataItem, "EditAccess") %></td>
			<td align="center"><%# DataBinder.Eval(Container.DataItem, "DeleteAccess") %></td>
			<td align="center"><%# DataBinder.Eval(Container.DataItem, "UploadAccess") %></td>
			<td width=15% style="font-weight:normal">
				<asp:linkbutton runat='server' commandname='edit' commandargument='<%# DataBinder.Eval(Container.DataItem, "AccessMaskID") %>'>Edit</asp:linkbutton>
				|
				<asp:linkbutton runat='server' onload="Delete_Load" commandname='delete' commandargument='<%# DataBinder.Eval(Container.DataItem, "AccessMaskID") %>'>Delete</asp:linkbutton>
			</td>
		</tr>
	
</ItemTemplate>
</asp:repeater>
<tr class="footer1">
	<td colSpan="12">
		<asp:linkbutton id="New" runat="server" text="New Access Mask"/>
	</td>
</tr>
</table>
		
</yaf:adminmenu>

<yaf:savescrollpos runat="server"/>
