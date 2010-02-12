<%@ Control Language="C#" AutoEventWireup="true" CodeFile="messagehistory.ascx.cs" Inherits="YAF.Pages.messagehistory" %>
<%@ Import Namespace="YAF.Classes.Core"%>
<YAF:PageLinks runat="server" ID="PageLinks" />
<table class="content" width="100%" cellspacing="1" cellpadding="0">
	<tr>
		<td class="header1" colspan="2">
			<YAF:LocalizedLabel ID="LocalizedLabel1" runat="server" LocalizedTag="title" />
		</td>
	</tr>	
	<asp:Repeater ID="RevisionsList" runat="server">
		<ItemTemplate>
		    <tr runat="server" id="history_tr" visible='<%# DataBinder.Eval( Container.DataItem, "Edited").ToString() !=  DataBinder.Eval( Container.DataItem, "Posted").ToString() %>' class="postheader"  >		           
					<td   colspan="1" class="header2">
					&nbsp					
					</td>
					<td id="history_column"  colspan="1" class='<%# Convert.ToInt32(Eval("IsModeratorChanged")) == 0 ? "postheader" :"post_res" %>' runat="server" >	
						<b>
							<YAF:LocalizedLabel ID="LocalizedLabel3" runat="server" LocalizedPage="POSTMESSAGE" LocalizedTag="EDITED" />
						</b>:
						<%# YafServices.DateTime.FormatDateTimeTopic( ( System.DateTime ) DataBinder.Eval( Container.DataItem, "Edited" ) ) %>
						<br />
						<b>
						<YAF:LocalizedLabel ID="LocalizedLabel5" runat="server" LocalizedPage="POSTMESSAGE" LocalizedTag="EDITEDBY" />
						</b>
						<YAF:UserLink ID="UserLink2" runat="server" UserID='<%# DataBinder.Eval(Container.DataItem, "EditedBy") %>' />
						<br />
						<b>
						<YAF:LocalizedLabel ID="LocalizedLabel4" runat="server" LocalizedPage="POSTMESSAGE" LocalizedTag="EDITREASON" />
						</b>
						<%# string.IsNullOrEmpty(Eval("EditReason").ToString())? this.PageContext.Localization.GetText("EDIT_REASON_NA"):Eval("EditReason") %>
						<br />
						<b>	
					  <%# PageContext.IsAdmin || (PageContext.BoardSettings.AllowModeratorsViewIPs && PageContext.IsModerator) ? "IP:" + Eval("IP") : ""%>
					   </b>
					</td>		
		    </tr>
			<tr runat="server" id="original_tr" visible='<%# (DataBinder.Eval( Container.DataItem, "Edited").ToString() == DataBinder.Eval( Container.DataItem, "Posted").ToString()) %>' class="postheader" >
					<td class="header2" colspan="1">					
				       <YAF:LocalizedLabel ID="LocalizedLabel6" runat="server" LocalizedPage="MESSAGEHISTORY" LocalizedTag="ORIGINALMESSAGE" ></YAF:LocalizedLabel>					
					</td>					
					<td id="original_column"  colspan="1" class='<%#  Convert.ToInt32(Eval("IsModeratorChanged")) == 0 ?  "postheader" :"post_res" %>' runat="server" >					
						<b>
						<YAF:UserLink ID="UserLink1" runat="server" UserID='<%# DataBinder.Eval(Container.DataItem, "UserID") %>' />
						</b>
						<YAF:OnlineStatusImage id="OnlineStatusImage" runat="server" Visible='<%# PageContext.BoardSettings.ShowUserOnlineStatus && !UserMembershipHelper.IsGuestUser( DataBinder.Eval(Container.DataItem, "UserID") )%>' Style="vertical-align: bottom" UserID='<%# DataBinder.Eval(Container.DataItem, "UserID") %>'  />						   
						&nbsp;
						<b>
							<YAF:LocalizedLabel ID="LocalizedLabel2" runat="server" LocalizedTag="POSTED" />
						</b>
						<%# YafServices.DateTime.FormatDateTimeTopic( ( System.DateTime ) DataBinder.Eval( Container.DataItem, "Posted" ) )%>
						&nbsp;
						<b>
						<%# PageContext.IsAdmin || (PageContext.BoardSettings.AllowModeratorsViewIPs && PageContext.IsModerator) ? "IP:" + Eval("IP") : ""%>
						</b>						
					</td>								
			</tr>									
			<tr>					
				<td class="post" colspan="2" align="center">
						<YAF:MessagePostData ID="MessagePostPrimary" runat="server" ShowAttachments="false"
							ShowSignature="false" DataRow="<%# PageContext.IsAdmin || PageContext.IsModerator ? (System.Data.DataRowView)Container.DataItem : null %>" >
						</YAF:MessagePostData>
				</td>				
			</tr>
				<tr runat="server" id="historystart_tr" visible='<%# (DataBinder.Eval( Container.DataItem, "Edited").ToString() == DataBinder.Eval( Container.DataItem, "Posted").ToString()) %>' class="postheader" >
					<td class="header2" colspan="2">					
				       <YAF:LocalizedLabel ID="LocalizedLabel8" runat="server" LocalizedPage="MESSAGEHISTORY" LocalizedTag="HISTORYSTART" ></YAF:LocalizedLabel>					
					</td>										
			</tr>
		</ItemTemplate>
	</asp:Repeater>
		<asp:Repeater ID="CurrentMessageRpt" visible="false" runat="server">
		<ItemTemplate>
			<tr class="postheader">
					<td class="header2" colspan="1" valign="top">
				       <YAF:LocalizedLabel ID="LocalizedLabel4" runat="server" LocalizedPage="MESSAGEHISTORY" LocalizedTag="CURRENTMESSAGE" />
					</td>
					<td  colspan="1" class='<%#  Convert.ToInt32(Eval("IsModeratorChanged")) == 0 ?  "postheader" :"post_res" %>' runat="server" >
						<b>
							<YAF:LocalizedLabel ID="LocalizedLabel3" runat="server" LocalizedPage="POSTMESSAGE" LocalizedTag="EDITED" />
						</b>
						<%# YafServices.DateTime.FormatDateTimeTopic( ( System.DateTime ) DataBinder.Eval( Container.DataItem, "Edited" ) ) %>
						<br />
						<b>
						<YAF:LocalizedLabel ID="LocalizedLabel5" runat="server" LocalizedPage="POSTMESSAGE" LocalizedTag="EDITEDBY" />
						</b>
						<YAF:UserLink ID="UserLink2" runat="server" UserID='<%# DataBinder.Eval(Container.DataItem, "EditedBy") %>' />
						<br />
						<b>
						<YAF:LocalizedLabel ID="LocalizedLabel7" runat="server" LocalizedPage="POSTMESSAGE" LocalizedTag="EDITREASON" />
						</b>				
						<%# string.IsNullOrEmpty(Eval("EditReason").ToString())? this.PageContext.Localization.GetText("EDIT_REASON_NA"):Eval("EditReason") %>
						<br />	
					 	<b>
						<%# PageContext.IsAdmin || (PageContext.BoardSettings.AllowModeratorsViewIPs && PageContext.IsModerator) ? "IP:" + Eval("IP") : ""%>
						</b>
						<br />
					</td>					
			</tr>			
			<tr>					
				<td class="post" colspan="2" align="center">
						<YAF:MessagePostData ID="MessagePostCurrent" runat="server" ShowAttachments="false"
							ShowSignature="false" DataRow="<%# PageContext.IsAdmin || PageContext.IsModerator ? (System.Data.DataRowView)Container.DataItem : null %>" >
						</YAF:MessagePostData>
				</td>				
			</tr>
		</ItemTemplate>
	</asp:Repeater>
	<tr class="postfooter">	
		<td class="post" colspan="2">
			<YAF:ThemeButton  ID="ReturnBtn" CssClass="yafcssbigbutton leftItem" OnClick="ReturnBtn_OnClick"  TextLocalizedTag="TOMESSAGE" Visible="false" runat="server"></YAF:ThemeButton>			
			<YAF:ThemeButton  ID="ReturnModBtn" CssClass="yafcssbigbutton leftItem" OnClick="ReturnModBtn_OnClick"  TextLocalizedTag="GOMODERATE"  Visible="false" runat="server"></YAF:ThemeButton>
		</td>
	</tr>	
</table>

<div id="DivSmartScroller">
	<YAF:SmartScroller ID="SmartScroller1" runat="server" />
</div>
