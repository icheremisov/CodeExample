// using Client.Core.Common.Contracts;
//
// namespace Client.Core.Common.UI.SystemNotification {
//
// 	public class SystemNotification : ISystemNotification {
// 		private readonly SystemNotificationView _view;
// 		
// 		public SystemNotification(SystemNotificationView view) {
// 			_view = view;
// 			_view.SetVisible(false);
// 		} 
// 		
// 		public void ShowNotification(string text) => _view.Show(text);
// 		public void ShowNotification(string title, string text) => _view.Show(title, text);
// 	}
//
// }