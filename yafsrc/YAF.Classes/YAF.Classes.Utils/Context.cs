/* Yet Another Forum.NET
 * Copyright (C) 2006-2009 Jaben Cargman
 * http://www.yetanotherforum.net/
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.Security;
using System.Threading;
using System.Globalization;
using YAF.Classes.Data;

namespace YAF.Classes.Utils
{
	/// <summary>
	/// Context class that accessable with the same instance from all locations
	/// </summary>
	public class YafContext
	{
		/* Ederon : 6/16/2007 - conventions */	
		
		private System.Data.DataRow _page = null;
		private YAF.Classes.Utils.YafTheme _theme = null;
		private YAF.Classes.Utils.YafLocalization _localization = null;
		private System.Web.Security.MembershipUser _user = null;
		private QueryStringIDHelper _queryStringIdHelper = null;
		private string _loadString = "";
		private string _adminLoadString = "";
		private UserFlags _userFlags = null;

		// init flags
		private bool _initCulture = false;
		private bool _initTheme = false;
		private bool _initLocalization = false;
		private bool _initUserPage = false;

		#region Load Message
		public string LoadString
		{
			get
			{
				if ( HttpContext.Current.Session ["LoadMessage"] != null )
				{
					// get this as the current "loadstring"
					_loadString = HttpContext.Current.Session ["LoadMessage"].ToString();
					// session load string no longer needed
					HttpContext.Current.Session ["LoadMessage"] = null;
				}
				return _loadString;
			}
		}

		public string LoadStringJavascript
		{
			get
			{
				string message = LoadString;
				message = message.Replace( "\\", "\\\\" );
				message = message.Replace( "'", "\\'" );
				message = message.Replace( "\r\n", "\\r\\n" );
				message = message.Replace( "\n", "\\n" );
				message = message.Replace( "\"", "\\\"" );
				return message;
			}
		}

		public string AdminLoadString
		{
			get
			{
				return _adminLoadString;
			}
		}

		/// <summary>
		/// AddLoadMessage creates a message that will be returned on the next page load.
		/// </summary>
		/// <param name="message">The message you wish to display.</param>
		public void AddLoadMessage( string message )
		{
			//message = message.Replace("\\", "\\\\");
			//message = message.Replace( "'", "\\'" );
			//message = message.Replace( "\r\n", "\\r\\n" );
			//message = message.Replace( "\n", "\\n" );
			//message = message.Replace( "\"", "\\\"" );

			_loadString += message + "\n\n";
		}

		/// <summary>
		/// AddLoadMessageSession creates a message that will be returned on the next page.
		/// </summary>
		/// <param name="message">The message you wish to display.</param>
		public void AddLoadMessageSession( string message )
		{
			HttpContext.Current.Session ["LoadMessage"] = message + "\r\n";
		}

		public void ClearLoadString()
		{
			string ls = this.LoadString;
			_loadString = string.Empty;
		}

		/// <summary>
		/// Instead of showing error messages in a pop-up javascript window every time
		/// the page loads (in some cases) provide a error message towards the bottom 
		/// of the page.
		/// </summary>
		public void AddAdminMessage( string errorType, string errorMessage )
		{
			_adminLoadString = string.Format( "<div style=\"margin: 2%; padding: 7px; border: 3px Solid Red; background-color: #ccc;\"><h1>{0}</h1>{1}</div>", errorType, errorMessage );
		}

		public void ResetLoadStrings()
		{
			_loadString = "";
			_adminLoadString = "";
		} 
		#endregion

		private static YafContext _currentInstance = new YafContext();
		public static YafContext Current
		{
			get
			{
				Page currentPage = HttpContext.Current.Handler as Page;

				if ( currentPage == null )
				{
					// only really used for the send mail thread.
					// since it's not inside a page. An instance is
					// returned that's for the whole process.
					return _currentInstance;
				}

				// save the yafContext in the currentpage items or just retreive from the page context
				return ( currentPage.Items ["YafContextPage"] ?? ( currentPage.Items ["YafContextPage"] = new YafContext() ) ) as YafContext;
			}
		}

		private string _transPage = string.Empty;
		/// <summary>
		/// Current TransPage for Localization
		/// </summary>
		public string TranslationPage
		{
			get { return _transPage; }
			set { _transPage = value; }
		}

		public System.Data.DataRow Page
		{
			get
			{
				if ( !_initUserPage ) InitUserAndPage();
				return _page;
			}
			set
			{
				_page = value;
				_initUserPage = ( value != null );

				// get user flags
				if (_page != null) _userFlags = new UserFlags(_page["UserFlags"]);
				else _userFlags = null;
			}
		}

		public YAF.Classes.Utils.YafUserProfile Profile
		{
			get
			{
				return ( YafUserProfile ) HttpContext.Current.Profile;
			}
		}

		public YAF.Classes.YafControlSettings Settings
		{
			get
			{
				return YafControlSettings.Current;
			}
		}

		public YAF.Classes.Utils.YafTheme Theme
		{
			get
			{			
				if ( !_initTheme ) InitTheme();
				return _theme;
			}
			set
			{
				_theme = value;
				_initTheme = ( value != null );
			}
		}

		public YAF.Classes.Utils.YafLocalization Localization
		{
			get
			{
				if ( !_initLocalization ) InitLocalization();
				if ( !_initCulture ) InitCulture();
				return _localization;
			}
			set
			{
				_localization = value;
				_initLocalization = ( value != null );
			}
		}

		public System.Web.Security.MembershipUser User
		{
			get
			{
				if ( _user == null )
					_user = System.Web.Security.Membership.GetUser();
				return _user;
			}
			set
			{
				_user = value;
			}
		}

		public QueryStringIDHelper QueryIDs
		{
			get
			{
				return _queryStringIdHelper;
			}
			set
			{
				_queryStringIdHelper = value;
			}
		}

		public YafBoardSettings BoardSettings
		{
			get
			{
				string key = YafCache.GetBoardCacheKey(Constants.Cache.BoardSettings);

				if ( HttpContext.Current.Application [key] == null )
					HttpContext.Current.Application [key] = new YafBoardSettings( PageBoardID );

				return ( YafBoardSettings ) HttpContext.Current.Application [key];
			}
			set
			{
				string key = YafCache.GetBoardCacheKey(Constants.Cache.BoardSettings);

				if ( value == null )
					HttpContext.Current.Application.Remove( key );
				else
				{
					// set the updated board settings...	
					HttpContext.Current.Application[key] = value;
				}
			}
		}

		/// <summary>
		/// Helper function to get a profile from the system
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>
		public YafUserProfile GetProfile( string userName )
		{
			return YafUserProfile.Create( userName ) as YafUserProfile;
		}

		/// <summary>
		/// Get the current page as the forumPage Enum (for comparison)
		/// </summary>
		public ForumPages ForumPageType
		{
			get
			{
				if (HttpContext.Current.Request.QueryString ["g"] == null)
					return ForumPages.forum;
				
				try
				{
					return ( ForumPages ) Enum.Parse( typeof( ForumPages ), HttpContext.Current.Request.QueryString ["g"], true );
				}
				catch ( Exception )
				{
					return ForumPages.forum;
				}
			}
		}

		/// <summary>
		/// Helper function to see if the Page variable is populated
		/// </summary>
		public bool PageIsNull()
		{
			return ( Page == null );
		}

		/// <summary>
		/// Helper function used for redundant "access" fields internally
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		private bool AccessNotNull( string field )
		{
			if ( Page [field] == DBNull.Value ) return false;
			return ( Convert.ToInt32( Page [field] ) > 0 );
		}

		/// <summary>
		/// Internal helper function used for redundant page variable access (bool)
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		private bool PageValueAsBool( string field )
		{
			if ( Page != null && Page [field] != DBNull.Value )
				return Convert.ToInt32( Page [field] ) != 0;

			return false;
		}

		/// <summary>
		/// Internal helper function used for redundant page variable access (int)
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		private int PageValueAsInt( string field )
		{
			if ( Page != null && Page [field] != DBNull.Value )
				return Convert.ToInt32( Page [field] );

			return 0;
		}

		/// <summary>
		/// Internal helper function used for redudant page variable access (string)
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		private string PageValueAsString( string field )
		{
			if ( Page != null && Page [field] != DBNull.Value )
				return Page [field].ToString();

			return "";
		}


		#region Forum and Page Helper Properties
		/// <summary>
		/// True if current user has post access in the current forum
		/// </summary>
		public bool ForumPostAccess
		{
			get
			{
				return AccessNotNull( "PostAccess" );
			}
		}
		/// <summary>
		/// True if the current user has reply access in the current forum
		/// </summary>
		public bool ForumReplyAccess
		{
			get
			{
				return AccessNotNull( "ReplyAccess" );
			}
		}
		/// <summary>
		/// True if the current user has read access in the current forum
		/// </summary>
		public bool ForumReadAccess
		{
			get
			{
				return AccessNotNull( "ReadAccess" );
			}
		}
		/// <summary>
		/// True if the current user has access to create priority topics in the current forum
		/// </summary>
		public bool ForumPriorityAccess
		{
			get
			{
				return AccessNotNull( "PriorityAccess" );
			}
		}
		/// <summary>
		/// True if the current user has access to create polls in the current forum.
		/// </summary>
		public bool ForumPollAccess
		{
			get
			{
				return AccessNotNull( "PollAccess" );
			}
		}
		/// <summary>
		/// True if the current user has access to vote on polls in the current forum
		/// </summary>
		public bool ForumVoteAccess
		{
			get
			{
				return AccessNotNull( "VoteAccess" );
			}
		}
		/// <summary>
		/// True if the current user is a moderator of the current forum
		/// </summary>
		public bool ForumModeratorAccess
		{
			get
			{
				return AccessNotNull( "ModeratorAccess" );
			}
		}
		/// <summary>
		/// True if the current user can delete own messages in the current forum
		/// </summary>
		public bool ForumDeleteAccess
		{
			get
			{
				return AccessNotNull( "DeleteAccess" );
			}
		}
		/// <summary>
		/// True if the current user can edit own messages in the current forum
		/// </summary>
		public bool ForumEditAccess
		{
			get
			{
				return AccessNotNull( "EditAccess" );
			}
		}
		/// <summary>
		/// True if the current user can upload attachments
		/// </summary>
		public bool ForumUploadAccess
		{
			get
			{
				return AccessNotNull( "UploadAccess" );
			}
		}
		/// <summary>
		/// True if the current user can download attachments
		/// </summary>
		public bool ForumDownloadAccess
		{
			get
			{
				return AccessNotNull("DownloadAccess");
			}
		}

		public int PageBoardID
		{
			get
			{
				return Settings == null ? 1 : Settings.BoardID;
			}
		}
		/// <summary>
		/// The UserID of the current user.
		/// </summary>
		public int PageUserID
		{
			get
			{
				return PageValueAsInt( "UserID" );
			}
		}
		public string PageUserName
		{
			get
			{
				return PageValueAsString( "UserName" );
			}
		}
		/// <summary>
		/// ForumID for the current page, or 0 if not in any forum
		/// </summary>
		public int PageForumID
		{
			get
			{
				int nLockedForum = Settings.LockedForum;
				if ( nLockedForum != 0 )
					return nLockedForum;

				return PageValueAsInt( "ForumID" );
			}
		}
		/// <summary>
		/// Name of forum for the current page, or an empty string if not in any forum
		/// </summary>
		public string PageForumName
		{
			get
			{
				return PageValueAsString( "ForumName" );
			}
		}
		/// <summary>
		/// CategoryID for the current page, or 0 if not in any category
		/// </summary>
		public int PageCategoryID
		{
			get
			{
				if ( Settings.CategoryID != 0 )
				{
					return Settings.CategoryID;
				}

				return PageValueAsInt( "CategoryID" );
			}
		}
		/// <summary>
		/// Name of category for the current page, or an empty string if not in any category
		/// </summary>
		public string PageCategoryName
		{
			get
			{
				return PageValueAsString( "CategoryName" );
			}
		}
		/// <summary>
		/// The TopicID of the current page, or 0 if not in any topic
		/// </summary>
		public int PageTopicID
		{
			get
			{
				return PageValueAsInt( "TopicID" );
			}
		}
		/// <summary>
		/// Name of topic for the current page, or an empty string if not in any topic
		/// </summary>
		public string PageTopicName
		{
			get
			{
				return PageValueAsString( "TopicName" );
			}
		}

		/// <summary>
		/// Is the current user host admin?
		/// </summary>
		public bool IsHostAdmin
		{
			get
			{
				bool isHostAdmin = false;

				if (_userFlags != null)
				{
					isHostAdmin = _userFlags.IsHostAdmin;
					// Obsolette : Ederon
					// if (General.BinaryAnd(Page["UserFlags"], UserFlags.IsHostAdmin))
					//	isHostAdmin = true;
				}

				return isHostAdmin;
			}
		}

		/// <summary>
		/// True if user is excluded from CAPTCHA check.
		/// </summary>
		public bool IsCaptchaExcluded
		{
			get
			{
				bool isCaptchaExcluded = false;

				if (_userFlags != null)
				{
					isCaptchaExcluded = _userFlags.IsCaptchaExcluded;
				}

				return isCaptchaExcluded;
			}
		}

		/// <summary>
		/// True if current user is an administrator
		/// </summary>
		public bool IsAdmin
		{
			get
			{
				if ( IsHostAdmin )
					return true;

				return PageValueAsBool( "IsAdmin" );
			}
		}
		/// <summary>
		/// True if the current user is a guest
		/// </summary>
		public bool IsGuest
		{
			get
			{
				return PageValueAsBool( "IsGuest" );
			}
		}
		/// <summary>
		/// True if the current user is a forum moderator (mini-admin)
		/// </summary>
		public bool IsForumModerator
		{
			get
			{
				return PageValueAsBool( "IsForumModerator" );
			}
		}
		/// <summary>
		/// True if current user is a modeator for at least one forum
		/// </summary>
		public bool IsModerator
		{
			get
			{
				return PageValueAsBool( "IsModerator" );
			}
		}

		/// <summary>
		/// True if the current user is suspended
		/// </summary>
		public bool IsSuspended
		{
			get
			{
				if ( Page != null && Page ["Suspended"] != DBNull.Value )
					return true;

				return false;
			}
		}

		/// <summary>
		/// When the user is suspended until
		/// </summary>
		public DateTime SuspendedUntil
		{
			get
			{
				if ( Page == null || Page ["Suspended"] == DBNull.Value )
					return DateTime.Now;
				else
					return Convert.ToDateTime( Page ["Suspended"] );
			}
		}

		/// <summary>
		/// The number of private messages that are unread
		/// </summary>
		public int UnreadPrivate
		{
			get
			{
				return Convert.ToInt32( Page ["Incoming"] );
			}
		}

		/// <summary>
		/// The time zone offset for the user
		/// </summary>
		public int TimeZoneUser
		{
			get
			{
				return Convert.ToInt32( Page ["TimeZoneUser"] );
			}
		}

		/// <summary>
		/// The language file for the user
		/// </summary>
		public string LanguageFile
		{
			get
			{
				return PageValueAsString( "LanguageFile" );
			}
		}

		/// <summary>
		/// True if board is private (20050909 CHP)
		/// </summary>
		public bool IsPrivate
		{
			get
			{
#if TODO
				try
				{
					return
						int.Parse(Utils.UtilsSection[string.Format("isprivate{0}", PageBoardID)])!=0;
				}
				catch
				{
					return false;
				}
#else
				return false;
#endif
			}
		}
		#endregion

		#region Init Functions

		/// <summary>
		/// Set the culture and UI culture to the browser's accept language
		/// </summary>
		protected void InitCulture()
		{
			if ( !_initCulture )
			{
				try
				{
					string cultureCode = "";
					
					/*string [] tmp = HttpContext.Current.Request.UserLanguages;
					if ( tmp != null )
					{
						cultureCode = tmp [0];
						if ( cultureCode.IndexOf( ';' ) >= 0 )
						{
							cultureCode = cultureCode.Substring( 0, cultureCode.IndexOf( ';' ) ).Replace( '_', '-' );
						}
					}
					else
					{
						cultureCode = "en-US";
					}*/

					cultureCode = _localization.LanguageCode;

					Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture( cultureCode );
					Thread.CurrentThread.CurrentUICulture = new CultureInfo( cultureCode );
				}
#if DEBUG
			catch ( Exception ex )
			{
				YAF.Classes.Data.DB.eventlog_create( this.PageUserID, this, ex );
				throw new ApplicationException( "Error getting User Language." + Environment.NewLine + ex.ToString() );
			}
#else
				catch ( Exception )
				{
					// set to default...
					Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture( "en-US" );
					Thread.CurrentThread.CurrentUICulture = new CultureInfo( "en-US" );
				}
#endif
				// mark as setup...
				_initCulture = true;
			}			
		}

		/// <summary>
		/// Set up the localization
		/// </summary>
		protected void InitLocalization()
		{
			if ( !_initLocalization )
			{
				this.Localization = new YAF.Classes.Utils.YafLocalization( this.TranslationPage );
			}			
		}

		/// <summary>
		/// Sets the theme class up for usage
		/// </summary>
		protected void InitTheme()
		{
			if ( !_initTheme )
			{
				string themeFile = null;

				if ( this.Page != null && this.Page ["ThemeFile"] != DBNull.Value && this.BoardSettings.AllowUserTheme )
				{
					// use user-selected theme
					themeFile = this.Page ["ThemeFile"].ToString();
				}
				else if ( this.Page != null && this.Page ["ForumTheme"] != DBNull.Value )
				{
					themeFile = this.Page ["ForumTheme"].ToString();
				}
				else
				{
					themeFile = this.BoardSettings.Theme;
				}

				if ( !YafTheme.IsValidTheme( themeFile ) )
				{
					themeFile = "yafpro.xml";
				}

				// create the theme class
				this.Theme = new YafTheme( themeFile );

				// make sure it's valid again...
				if ( !YafTheme.IsValidTheme( this.Theme.ThemeFile ) )
				{
					// can't load a theme... throw an exception.
					throw new Exception( String.Format( "Unable to find a theme to load. Last attempted to load \"{0}\" but failed.", themeFile ) );
				}
			}
		}

		/// <summary>
		/// Initialize the user data and page data...
		/// </summary>
		protected void InitUserAndPage()
		{
			if ( !_initUserPage )
			{
				try
				{
					System.Data.DataRow pageRow;

					// Find user name
					MembershipUser user = Membership.GetUser();
					if ( user != null && HttpContext.Current.Session ["UserUpdated"] == null )
					{
						RoleMembershipHelper.UpdateForumUser( user, this.PageBoardID );
						HttpContext.Current.Session ["UserUpdated"] = true;
					}

					string browser = String.Format( "{0} {1}", HttpContext.Current.Request.Browser.Browser, HttpContext.Current.Request.Browser.Version );
					string platform = HttpContext.Current.Request.Browser.Platform;
					bool isSearchEngine = false;

					if ( HttpContext.Current.Request.UserAgent != null )
					{
						if ( HttpContext.Current.Request.UserAgent.IndexOf( "Windows NT 5.2" ) >= 0 )
						{
							platform = "Win2003";
						}
						else if ( HttpContext.Current.Request.UserAgent.IndexOf( "Windows NT 6.0" ) >= 0 )
						{
							platform = "Vista";
						}
						else
						{
							// check if it's a search engine spider...
							isSearchEngine = General.IsSearchEngineSpider( HttpContext.Current.Request.UserAgent );
						}
					}

					int? categoryID = General.ValidInt( HttpContext.Current.Request.QueryString ["c"] );
					int? forumID = General.ValidInt( HttpContext.Current.Request.QueryString ["f"] );
					int? topicID = General.ValidInt( HttpContext.Current.Request.QueryString ["t"] );
					int? messageID = General.ValidInt( HttpContext.Current.Request.QueryString ["m"] );

					if ( this.Settings.CategoryID != 0 )
						categoryID = this.Settings.CategoryID;

					object userKey = DBNull.Value;

					if ( user != null )
					{
						userKey = user.ProviderUserKey;
					}

					do
					{
						pageRow = DB.pageload(
								HttpContext.Current.Session.SessionID,
								this.PageBoardID,
								userKey,
								HttpContext.Current.Request.UserHostAddress,
								HttpContext.Current.Request.FilePath,
								browser,
								platform,
								categoryID,
								forumID,
								topicID,
								messageID,
							// don't track if this is a search engine
								isSearchEngine );

						// if the user doesn't exist...
						if ( user != null && pageRow == null )
						{
							// create the user...
							if ( !RoleMembershipHelper.DidCreateForumUser( user, this.PageBoardID ) )
								throw new ApplicationException( "Failed to use new user." );
						}

						// only continue if either the page has been loaded or the user has been found...
					} while ( pageRow == null && user != null );

					// page still hasn't been loaded...
					if ( pageRow == null )
					{
						throw new ApplicationException( "Failed to find guest user." );
					}

					// save this page data to the context...
					this.Page = pageRow;
				}
				catch (Exception x)
				{
#if !DEBUG
					// log the exception...
					YAF.Classes.Data.DB.eventlog_create( null, "Failure Initializing User/Page.", x, EventLogTypes.Warning );
					// show a failure notice since something is probably up with membership...
					YafBuildLink.RedirectInfoPage( InfoMessage.Failure );
#else
					// re-throw exception...
					throw;
#endif
				}
			}
		}

		#endregion
	}

	/// <summary>
	/// Class provides misc helper functions and forum version information
	/// </summary>
	public static class YafForumInfo
	{
		/// <summary>
		/// The forum path (external).
		/// May not be the actual URL of the forum.
		/// </summary>
		static public string ForumRoot
		{
			get
			{
				string _forumRoot = null;

				try
				{
					_forumRoot = UrlBuilder.BaseUrl;
					if ( !_forumRoot.EndsWith( "/" ) ) _forumRoot += "/";
				}
				catch ( Exception )
				{
					_forumRoot = "/";
				}

				return _forumRoot;
			}			
		}

		/// <summary>
		/// The forum path (internal).
		/// May not be the actual URL of the forum.
		/// </summary>
		static public string ForumFileRoot
		{
			get
			{
				return UrlBuilder.RootUrl;
			}
		}

		/// <summary>
		/// Server URL based on the server variables. May not actually be 
		/// the URL of the forum.
		/// </summary>
		static public string ServerURL
		{
			get
			{
				StringBuilder url = new StringBuilder();

				if ( !Config.BaseUrlOverrideDomain )
				{
					long serverPort = long.Parse( HttpContext.Current.Request.ServerVariables ["SERVER_PORT"] );
					bool isSecure = ( HttpContext.Current.Request.ServerVariables ["HTTPS"] == "ON" || serverPort == 443 );

					url.Append( "http" );

					if ( isSecure )
					{
						url.Append( "s" );
					}

					url.AppendFormat( "://{0}", HttpContext.Current.Request.ServerVariables ["SERVER_NAME"] );

					if ( ( !isSecure && serverPort != 80 ) || ( isSecure && serverPort != 443 ) )
					{
						url.AppendFormat( ":{0}", serverPort.ToString() );
					}					
				}
				else
				{
					// pull the domain from BaseUrl...
					string [] sections = UrlBuilder.BaseUrl.Split( new char [] { '/' } );

					// add the necessary sections...
					// http(s)
					url.Append( sections [0] );
					url.Append( "//" );
					url.Append( sections [1] );
				}

				return url.ToString();
			}
		}

		/// <summary>
		/// Complete external URL of the forum.
		/// </summary>
		static public string ForumBaseUrl
		{
			get
			{
				if ( !Config.BaseUrlOverrideDomain )
				{
					return ServerURL + ForumRoot;
				}
				else
				{
					// just return the base url...
					return UrlBuilder.BaseUrl;
				}		
			}
		}	

		static public string ForumURL
		{
			get
			{
				if ( !Config.BaseUrlOverrideDomain )
				{
					return string.Format( "{0}{1}", YafForumInfo.ServerURL, YafBuildLink.GetLink( ForumPages.forum ) );
				}
				else
				{
					// link will include the url and domain...
					return YafBuildLink.GetLink( ForumPages.forum );
				}
			}
		}

		static public bool IsLocal
		{
			get
			{
				string s = HttpContext.Current.Request.ServerVariables ["SERVER_NAME"];
				return s != null && s.ToLower() == "localhost";
			}
		}

		/// <summary>
		/// Helper function that creates the the url of a resource.
		/// </summary>
		/// <param name="resourceName"></param>
		/// <returns></returns>
		static public string GetURLToResource( string resourceName )
		{
			return string.Format( "{1}resources/{0}", resourceName, YafForumInfo.ForumRoot );
		}

		#region Version Information
		static public string AppVersionNameFromCode( long code )
		{
			string version;

			if ( ( code & 0xF0 ) > 0 || ( code & 0x0F ) == 1 )
			{
				version = String.Format( "{0}.{1}.{2}{3}", ( code >> 24 ) & 0xFF, ( code >> 16 ) & 0xFF, ( code >> 8 ) & 0xFF, Convert.ToInt32((( code >> 4 ) & 0x0F)).ToString("00") );
			}
			else
			{
				version = String.Format( "{0}.{1}.{2}", ( code >> 24 ) & 0xFF, ( code >> 16 ) & 0xFF, ( code >> 8 ) & 0xFF );
			}

			if ( ( code & 0x0F ) > 0 )
			{				
				if ( ( code & 0x0F ) == 1 )
				{
					// alpha release...
					version += " alpha";
				}
				else if ( ( code & 0x0F ) == 2 )
				{
					version += " beta";
				}
				else
				{
					// Add Release Candidate
					version += string.Format( " RC{0}", (code & 0x0F) - 2 );
				}
			}

			return version;
		}
		static public string AppVersionName
		{
			get
			{
				return AppVersionNameFromCode( AppVersionCode );
			}
		}
		static public int AppVersion
		{
			get
			{
				return 33;
			}
		}
		static public long AppVersionCode
		{
			get
			{
				return 0x01090300;
			}
		}
		static public DateTime AppVersionDate
		{
			get
			{
				return new DateTime( 2009, 4, 10 );
			}
		}
		#endregion
	}
}
