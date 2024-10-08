using RectEx;
using Sirenix.OdinInspector;
using UnityEngine;
using XLib.UI.Procedural.Brushes;

namespace XLib.UI.Procedural {

	public class UIRectangle : UIProceduralEffect {
		[SerializeField] private float _softness = 0;

		[Space]
		[SerializeField] private bool _fill = true;
		[SerializeField, ShowIf(nameof(_fill))] private Vector2Int _subdivide = new Vector2Int(1, 1);
		[BoxGroup("Color")]
		[SerializeField, ShowIf(nameof(_fill)), InlineProperty, HideLabel] private UIProceduralBrush _color;

		[Space]
		[SerializeField] private bool _border = false;
		[SerializeField, ShowIf(nameof(_border))] private float _borderSize = 1.0f;
		[BoxGroup("Border Color")]
		[SerializeField, ShowIf(nameof(_border)), InlineProperty, HideLabel] private UIProceduralBrush _borderColor;

		protected override void GenerateGraphics() {
			if (_border && _fill) {
				GenerateFill(graphic.rectTransform.rect.Extend(_borderSize - _softness * 2));
				GenerateBorder();
			}
			else if (_border)
				GenerateBorder();
			else if (_fill) GenerateFill(graphic.rectTransform.rect);
		}
		
		private void GenerateFill(Rect rect) {
			
			if (_subdivide.x < 1 || _subdivide.y < 1) return;
			
			var vOffset = _vertices.Count;
			GeometryUtils.GenerateGrid(_vertices, _indices, rect, _subdivide, graphic.color);
			ApplyColor(_color, rect, vOffset, _vertices.Count - vOffset);
		}

		private void GenerateBorder() {
			var owner = this.graphic;
			var rect = owner.rectTransform.rect;

			var vOffset = _vertices.Count;
			if (_softness <= 0.01f) {
				
				var columns = new[] {
					_borderSize, rect.size.x - _borderSize * 2, _borderSize,
				}; 
				var rows = new[] {
					_borderSize, rect.size.y - _borderSize * 2, _borderSize,
				};

				GeometryUtils.GenerateGrid(_vertices, _indices, rect.min, columns, rows, false, graphic.color);

			}
			else {

				var bColor = graphic.color;
				
				var empty = bColor.ReplaceA(0);
				
				var colors = new[] {
					empty, empty, empty, empty, empty, empty, empty, empty,
					empty, bColor, bColor, bColor, bColor, bColor, bColor, empty,
					empty, bColor, bColor, bColor, bColor, bColor, bColor, empty,
					empty, bColor, bColor, empty, empty, bColor, bColor, empty,
					empty, bColor, bColor, empty, empty, bColor, bColor, empty,
					empty, bColor, bColor, bColor, bColor, bColor, bColor, empty,
					empty, bColor, bColor, bColor, bColor, bColor, bColor, empty,
					empty, empty, empty, empty, empty, empty, empty, empty,
				};
				
				var columns = new[] {
					_softness, _borderSize - _softness * 2, _softness, rect.size.x - _borderSize * 2, _softness, _borderSize - _softness * 2, _softness,
				}; 
				var rows = new[] {
					_softness, _borderSize - _softness * 2, _softness, rect.size.y - _borderSize * 2, _softness, _borderSize - _softness * 2, _softness,
				};
				
				GeometryUtils.GenerateGrid(_vertices, _indices, rect.min, columns, rows, false, colors);
			}
			
			ApplyColor(_borderColor, rect, vOffset, _vertices.Count - vOffset);
		}

		// private void GenerateBorderFill() {
		// 	var owner = this.graphic;
		// 	var rect = owner.rectTransform.rect;
		//
		// 	var color = _color * owner.color;
		//
		// 	GeometryUtils.AddQuad(_vertices, _indices, 
		// 		rect.min + new Vector2(_borderSize, _borderSize),
		// 		rect.max - new Vector2(_borderSize, _borderSize), 
		// 		Vector2.zero, Vector2.one, color);
		// 	
		// 	var bColor = _borderColor * owner.color;
		// 	
		// 	if (_softness <= 0.01f) {
		// 		var colors = new[] {
		// 			bColor, bColor, bColor, bColor,
		// 			bColor, bColor, bColor, bColor,
		// 			bColor, bColor, bColor, bColor,
		// 			bColor, bColor, bColor, bColor,
		// 		};
		// 		
		// 		var columns = new[] {
		// 			_borderSize, rect.size.x - _borderSize * 2, _borderSize,
		// 		}; 
		// 		var rows = new[] {
		// 			_borderSize, rect.size.y - _borderSize * 2, _borderSize,
		// 		};
		//
		// 		GeometryUtils.GenerateGrid(_vertices, _indices, rect.min, columns, rows, colors, false);
		// 	}
		// 	else {
		// 		
		// 		var bColorN = bColor.ReplaceA(0);
		// 		
		// 		var colors = new[] {
		// 			bColorN, bColorN, bColorN, bColorN, bColorN , bColorN,
		// 			bColorN, bColor, bColor, bColor, bColor, bColorN,
		// 			bColorN, bColor, bColor, bColor, bColor, bColorN,
		// 			bColorN, bColor, bColor, bColor, bColor, bColorN,
		// 			bColorN, bColor, bColor, bColor, bColor, bColorN,
		// 			bColorN, bColorN, bColorN, bColorN, bColorN , bColorN,
		// 		};
		// 		
		// 		var columns = new[] {
		// 			_softness, _borderSize - _softness, rect.size.x - _borderSize * 2, _borderSize - _softness, _softness,
		// 		}; 
		// 		var rows = new[] {
		// 			_softness, _borderSize - _softness, rect.size.y - _borderSize * 2, _borderSize - _softness, _softness,
		// 		};
		//
		// 		GeometryUtils.GenerateGrid(_vertices, _indices, rect.min, columns, rows, colors, false);
		// 	}
		// }
	}

}