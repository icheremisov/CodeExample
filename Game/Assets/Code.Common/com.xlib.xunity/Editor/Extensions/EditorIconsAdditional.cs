using System;
using Sirenix.Utilities.Editor;

namespace XLib.Unity.Extensions {

	public static class EditorIconsAdditional {
		private static EditorIcon _paste;
		private static EditorIcon _copy;

		/// <summary>Gets an icon of a paste symbol.</summary>
		public static EditorIcon Paste {
			get {
				if (_paste != null) return _paste;
				_paste = new LazyEditorIcon(32, 32,
					"iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IB2cksfwAAAAlwSFlzAAALEwAACxMBAJqcGAAAAp9JREFUeJzNlz9oFUEQxvNM8hQhJAHTJCAiURAUIhI1hPinsNRgIYQUsbIRUSxEBauINhIEiWlsbKxVsLJQRBK0sIiiItFCJYhYqKAR/7yM3+fOwt66d7u+dwEPfnDv3e3MtzO7s3NNTYmXiFTAfvAC/AB3wfrU8Q1fcLYRLICb4Ah4prQspVPOejVYBy6AD2AFWAZ2gV9gGPSCVXy/DIcdYI/en1YnNeUr6FYBo87/9tl20Al2g9Z6nB8Hn8Br0AyuqXFei8obcB180d/2Gd8bA33gJ5gDO5Oios7PqJFFXWQrwXlHgCvCxRXAtOxz7DAqgykC1oJvjlEa2AAOagpCjn0ougcccwSQ2WgU8MIJZ5Do/UldcJPgAXhYwDQ4pGm740WGE9gUC/9kINTM8QE1mgJTNuFNxE5mb0zAlDfITcVLcC/CfTFb1HduBYzmpiFHgC8kRE3D61LLgTtrR1BEREDoss6vgiExez+PATCvYz4HRdQp4K0klGAxBeuVE7W/RdQp4BGNJwig7ceSTV1WRAMCohVObZ+SbC1h+j6C3lIiIGb7dXrwPGlV20zDYXAL3BZTUyjobMMCwKCYcuvvBsKzYLkXDY7ZrM+n/kSxQQFbxeQ0JOAJqAbS0leaAGelVwME10iZAiqO0UqMJYkAaAfnwKUExtRXqQJGxDQfoTXgw8OtuWwB3II8yscTGC47Av/FGuDYNjEdcVcBfF4tOwIUwNNwQdLWwFMdkyvA7YhSBfSLqespAmaKImBbqX8S4JXYGHY9ZAWokREJt1N5AmYl4TjOqYRbVMBlVwC73znJ9vpFAr6Dbd7MUuD7F1XA0ZCy95Lt6Yv6RH5HMK+xhtXluTpnl9ThC7AfolfAOylOScqHSgge3TfAGuv3N9y0ravhieogAAAAAElFTkSuQmCC");
				CleanupUtility.DisposeObjectOnAssemblyReload((IDisposable)_paste);
				return _paste;
			}
		}	
		
		/// <summary>Gets an icon of a paste symbol.</summary>
		public static EditorIcon Copy {
			get {
				if (_copy != null) return _copy;
				_copy = new LazyEditorIcon(32, 32,
					"iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IB2cksfwAAAAlwSFlzAAALEwAACxMBAJqcGAAAAppJREFUeJy1lztoFEEYx29XDBJBRGKVQrSQGBDPiPi4xsLeKkVKzVmKoKBWIjYBO4trfCFYCoKglocPkmDlo/J6QcFHEfQ81939/P/db8nkbmdv9uHAj9vb/Wb2P/M9dqbRsLQwDD0RaYIn4AeIpVxjvzXwCOzjuLZ3bmgwPq4dI4OwBGb/7+Cgq4D34A9YBSfATAVOgrcqYhnkr0IURT6MpsBOsKXT6fitVsvLI2889PdUSKSTmrYax3FM4z1gEZx35Bw4mjczPPM1luiWZp7RDfDb8J+rj3n9AGwuJUCj/rIx6DvwDDwdA20+aLSz38MsEWMF4OYE+KqDXGOHwWDg9fv9XGDDga+oAKsIFwGz+jAAkzZfZrR05fhisYlwEdDUhzTyCwhoaCBGsl6sRkT8bwF7wS/DDaaI+8yO0gLa7bZnI7VB3eALFsA3Ga1+zPsDpQTgdz9YkiQ1szgjmvsMWFxvBYclqQnHNJMo4jTYVEiAFqSVodkMw9nNZbkFac0Zv1C7xcICgiCggKvgJxhYeAO2WwR4KiAsJYD3dBXSAMrCWnprEVCl1SJAZ79Dkq+iC1Np38oCNLVuSvENyCsKryxA/f9cRvN6HNz1TNTlgl2SlNmLjlwAh2qLAaN5jtQbhGy9Xs8rQq0CJCmpt8A9R+6CecZPhgCW6UD/z7oG4euCAUj6Q0HIe2fBdb3+wueuK8AA/Ag+O/IJ3JGNaciXpjstjn+JKe4kgINwJYDvyD/7oRhI9wX8dixlBHh+EJZtWsjSFbgNdqfibAKmZb3ozHS7XbcznH28SXUJxzvl0sH8/vMYxeNU2aPYEfDY8P82V9VzkpTSKgdSsy8POAvcWzgJ4D5fku05j9JrUu5IHmvAveQq5vod7S+ZjXGcZ5ORiAAAAABJRU5ErkJggg==");
				CleanupUtility.DisposeObjectOnAssemblyReload((IDisposable)_copy);
				return _copy;
			}
		}
	}

}