package com.cell.ui.layout
{
	import com.cell.gfx.CellScreenManager;
	import com.cell.ui.ImageButton;
	import com.cell.ui.component.UIComponent;
	
	import flash.display.DisplayObject;
	import flash.display.Sprite;
	import flash.text.TextFormat;

	public class UILayoutManager
	{
		private static var instance : UILayoutManager;
		
		public static function getInstance() : UILayoutManager
		{
			return instance;
		}
		
//		---------------------------------------------------------------------------------------------------------

		private var _width 	: int = 320;
		private var _height : int = 480;
		private var _root 	: CellScreenManager;

		public function UILayoutManager(
			s:CellScreenManager, 
			width:int,
			height:int)
		{
			this._root		= s;
			this._width		= width;
			this._height 	= height;
			UILayoutManager.instance = this;
		}
		
		public function getRoot() : CellScreenManager {
			return _root;
		}
		
		public function get ScreenWidth() : int {
			return _width;
		}
		
		public function get ScreenHeight() : int {
			return _height;
		}
		
		
//		---------------------------------------------------------------------------------------------------------
		
		
		public function createUI(key:String, owner:*) : UIRect
		{
			return null;
		}
		
		public function createTextFormat(key:String, owner:*) : TextFormat
		{
			return new TextFormat("Verdana", 12, 0);
		}

		public function createButton(key:String, owner:*) : DisplayObject
		{
			return null;
		}
		

	
	
	
	}
}