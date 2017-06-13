// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ICSharpCode.NRefactory.TypeSystem;

namespace CShell.Completion.Images
{
	/// <summary>
	/// Provides icons for code-completion.
	/// </summary>
	public class CompletionImage
	{
		#region non-entity Images
		static readonly BitmapImage namespaceImage = LoadBitmap("NameSpace");
		
		/// <summary>
		/// Gets the image for namespaces.
		/// </summary>
		public static ImageSource NamespaceImage => namespaceImage;

	    static BitmapImage LoadBitmap(string name)
		{
			BitmapImage image = new BitmapImage(new Uri("pack://application:,,,/CShell.Completion;component/Images/" + name + ".png"));
			image.Freeze();
			return image;
		}
		#endregion
		
		#region Entity Images
		static readonly CompletionImage ImageClass = new CompletionImage("Class", false);
		static readonly CompletionImage ImageStruct = new CompletionImage("Struct", false);
		static readonly CompletionImage ImageInterface = new CompletionImage("Interface", false);
		static readonly CompletionImage ImageDelegate = new CompletionImage("Delegate", false);
		static readonly CompletionImage ImageEnum = new CompletionImage("Enum", false);
		static readonly CompletionImage ImageStaticClass = new CompletionImage("StaticClass", false);
		
		/// <summary>Gets the image used for non-static classes.</summary>
		public static CompletionImage Class => ImageClass;

	    /// <summary>Gets the image used for structs.</summary>
		public static CompletionImage Struct => ImageStruct;

	    /// <summary>Gets the image used for interfaces.</summary>
		public static CompletionImage Interface => ImageInterface;

	    /// <summary>Gets the image used for delegates.</summary>
		public static CompletionImage Delegate => ImageDelegate;

	    /// <summary>Gets the image used for enums.</summary>
		public static CompletionImage Enum => ImageEnum;

	    /// <summary>Gets the image used for modules/static classes.</summary>
		public static CompletionImage StaticClass => ImageStaticClass;

	    static readonly CompletionImage ImageField = new CompletionImage("Field", true);
		static readonly CompletionImage ImageFieldReadOnly = new CompletionImage("FieldReadOnly", true);
		static readonly CompletionImage ImageLiteral = new CompletionImage("Literal", false);
		static readonly CompletionImage ImageEnumValue = new CompletionImage("EnumValue", false);
		
		/// <summary>Gets the image used for non-static classes.</summary>
		public static CompletionImage Field => ImageField;

	    /// <summary>Gets the image used for structs.</summary>
		public static CompletionImage ReadOnlyField => ImageFieldReadOnly;

	    /// <summary>Gets the image used for constants.</summary>
		public static CompletionImage Literal => ImageLiteral;

	    /// <summary>Gets the image used for enum values.</summary>
		public static CompletionImage EnumValue => ImageEnumValue;

	    static readonly CompletionImage ImageMethod = new CompletionImage("Method", true);
		static readonly CompletionImage ImageConstructor = new CompletionImage("Constructor", true);
		static readonly CompletionImage ImageVirtualMethod = new CompletionImage("VirtualMethod", true);
		static readonly CompletionImage ImageOperator = new CompletionImage("Operator", false);
		static readonly CompletionImage ImageExtensionMethod = new CompletionImage("ExtensionMethod", true);
		static readonly CompletionImage ImagePInvokeMethod = new CompletionImage("PInvokeMethod", true);
		static readonly CompletionImage ImageProperty = new CompletionImage("Property", true);
		static readonly CompletionImage ImageIndexer = new CompletionImage("Indexer", true);
		static readonly CompletionImage ImageEvent = new CompletionImage("Event", true);
		
		/// <summary>Gets the image used for methods.</summary>
		public static CompletionImage Method => ImageMethod;

	    /// <summary>Gets the image used for constructos.</summary>
		public static CompletionImage Constructor => ImageConstructor;

	    /// <summary>Gets the image used for virtual methods.</summary>
		public static CompletionImage VirtualMethod => ImageVirtualMethod;

	    /// <summary>Gets the image used for operators.</summary>
		public static CompletionImage Operator => ImageOperator;

	    /// <summary>Gets the image used for extension methods.</summary>
		public static CompletionImage ExtensionMethod => ImageExtensionMethod;

	    /// <summary>Gets the image used for P/Invoke methods.</summary>
		public static CompletionImage PInvokeMethod => ImagePInvokeMethod;

	    /// <summary>Gets the image used for properties.</summary>
		public static CompletionImage Property => ImageProperty;

	    /// <summary>Gets the image used for indexers.</summary>
		public static CompletionImage Indexer => ImageIndexer;

	    /// <summary>Gets the image used for events.</summary>
		public static CompletionImage Event => ImageEvent;

	    /// <summary>
		/// Gets the CompletionImage instance for the specified entity.
		/// Returns null when no image is available for the entity type.
		/// </summary>
		public static CompletionImage GetCompletionImage(IEntity entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));
			switch (entity.EntityType) {
				case EntityType.TypeDefinition:
					return GetCompletionImageForType(((ITypeDefinition)entity).Kind, entity.IsStatic);
				case EntityType.Field:
					IField field = (IField)entity;
					if (field.IsConst) {
						if (field.DeclaringTypeDefinition != null && field.DeclaringTypeDefinition.Kind == TypeKind.Enum)
							return ImageEnumValue;
						else
							return ImageLiteral;
					}
					return field.IsReadOnly ? ImageFieldReadOnly : ImageField;
				case EntityType.Method:
					IMethod method = (IMethod)entity;
                    if (method.IsExtensionMethod)
                        return ImageExtensionMethod;
                    else
                        return method.IsOverridable ? ImageVirtualMethod : ImageMethod;
				case EntityType.Property:
					return ImageProperty;
				case EntityType.Indexer:
					return ImageIndexer;
				case EntityType.Event:
					return ImageEvent;
				case EntityType.Operator:
				case EntityType.Destructor:
					return ImageOperator;
				case EntityType.Constructor:
					return ImageConstructor;
				default:
					return null;
			}
		}
	
		/// <summary>
		/// Gets the CompletionImage instance for the specified entity.
		/// Returns null when no image is available for the entity type.
		/// </summary>
		public static CompletionImage GetCompletionImage(IUnresolvedEntity entity)
		{
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));
			switch (entity.SymbolKind) {
                case SymbolKind.TypeDefinition:
					return GetCompletionImageForType(((IUnresolvedTypeDefinition)entity).Kind, entity.IsStatic);
                case SymbolKind.Field:
					IUnresolvedField field = (IUnresolvedField)entity;
					if (field.IsConst) {
						if (field.DeclaringTypeDefinition != null && field.DeclaringTypeDefinition.Kind == TypeKind.Enum)
							return ImageEnumValue;
						else
							return ImageLiteral;
					}
					return field.IsReadOnly ? ImageFieldReadOnly : ImageField;
                case SymbolKind.Method:
					IUnresolvedMethod method = (IUnresolvedMethod)entity;
					return method.IsOverridable ? ImageVirtualMethod : ImageMethod;
                case SymbolKind.Property:
					return ImageProperty;
                case SymbolKind.Indexer:
					return ImageIndexer;
                case SymbolKind.Event:
					return ImageEvent;
                case SymbolKind.Operator:
                case SymbolKind.Destructor:
					return ImageOperator;
                case SymbolKind.Constructor:
					return ImageConstructor;
				default:
					return null;
			}
		}
		
		static CompletionImage GetCompletionImageForType(TypeKind typeKind, bool isStatic)
		{
			switch (typeKind) {
				case TypeKind.Interface:
					return ImageInterface;
				case TypeKind.Struct:
				case TypeKind.Void:
					return ImageStruct;
				case TypeKind.Delegate:
					return ImageDelegate;
				case TypeKind.Enum:
					return ImageEnum;
				case TypeKind.Class:
					return isStatic ? ImageStaticClass : ImageClass;
				case TypeKind.Module:
					return ImageStaticClass;
				default:
					return null;
			}
		}
		
		/// <summary>
		/// Gets the image for the specified entity.
		/// Returns null when no image is available for the entity type.
		/// </summary>
		public static ImageSource GetImage(IEntity entity)
		{
			CompletionImage image = GetCompletionImage(entity);
			if (image != null)
				return image.GetImage(entity.Accessibility, entity.IsStatic);
			else
				return null;
		}
		
		/// <summary>
		/// Gets the image for the specified entity.
		/// Returns null when no image is available for the entity type.
		/// </summary>
		public static ImageSource GetImage(IUnresolvedEntity entity)
		{
			CompletionImage image = GetCompletionImage(entity);
			if (image != null)
				return image.GetImage(entity.Accessibility, entity.IsStatic);
			else
				return null;
		}
		#endregion
		
		#region Overlays
		static readonly BitmapImage OverlayStatic = LoadBitmap("OverlayStatic");
		
		/// <summary>
		/// Gets the overlay image for the static modifier.
		/// </summary>
		public ImageSource StaticOverlay => OverlayStatic;

	    const int AccessibilityOverlaysLength = 5;
		
		static readonly BitmapImage[] AccessibilityOverlays = new BitmapImage[AccessibilityOverlaysLength] {
			null,
			LoadBitmap("OverlayPrivate"),
			LoadBitmap("OverlayProtected"),
			LoadBitmap("OverlayInternal"),
			LoadBitmap("OverlayProtectedInternal")
		};
		
		/// <summary>
		/// Gets an overlay image for the specified accessibility.
		/// Returns null if no overlay exists (for example, public members don't use overlays).
		/// </summary>
		public static ImageSource GetAccessibilityOverlay(Accessibility accessibility) => AccessibilityOverlays[GetAccessibilityOverlayIndex(accessibility)];

	    static int GetAccessibilityOverlayIndex(Accessibility accessibility)
		{
			switch (accessibility) {
				case Accessibility.Private:
					return 1;
				case Accessibility.Protected:
					return 2;
				case Accessibility.Internal:
					return 3;
				case Accessibility.ProtectedOrInternal:
				case Accessibility.ProtectedAndInternal:
					return 4;
				default:
					return 0;
			}
		}
		#endregion
		
		#region Instance Members (add overlay to entity image)
		readonly string _imageName;
		readonly bool _showStaticOverlay;
		
		private CompletionImage(string imageName, bool showStaticOverlay)
		{
			this._imageName = imageName;
			this._showStaticOverlay = showStaticOverlay;
		}
		
		ImageSource[] _images = new ImageSource[2 * AccessibilityOverlaysLength];
		// 0..N-1  = base image + accessibility overlay
		// N..2N-1 = base image + static overlay + accessibility overlay
		
		/// <summary>
		/// Gets the image without any overlays.
		/// </summary>
		public ImageSource BaseImage {
			get {
				ImageSource image = _images[0];
				if (image == null) {
					image = LoadBitmap(_imageName);
					Thread.MemoryBarrier();
					_images[0] = image;
				}
				return image;
			}
		}
		
		/// <summary>
		/// Gets this image combined with the specified accessibility overlay.
		/// </summary>
		public ImageSource GetImage(Accessibility accessibility, bool isStatic = false)
		{
			int accessibilityIndex = GetAccessibilityOverlayIndex(accessibility);
			int index;
			if (isStatic && _showStaticOverlay)
				index = AccessibilityOverlays.Length + accessibilityIndex;
			else
				index = accessibilityIndex;
			
			if (index == 0)
				return this.BaseImage;
			
			ImageSource image = _images[index];
			if (image == null) {
				DrawingGroup g = new DrawingGroup();
				Rect iconRect = new Rect(0, 0, 16, 16);
				g.Children.Add(new ImageDrawing(this.BaseImage, iconRect));
				
				if (AccessibilityOverlays[accessibilityIndex] != null)
					g.Children.Add(new ImageDrawing(AccessibilityOverlays[accessibilityIndex], iconRect));
				
				image = new DrawingImage(g);
				image.Freeze();
				Thread.MemoryBarrier();
				_images[index] = image;
			}
			return image;
		}
		
		/// <inheritdoc/>
		public override string ToString() => "[CompletionImage " + _imageName + "]";

	    #endregion
	}
}
