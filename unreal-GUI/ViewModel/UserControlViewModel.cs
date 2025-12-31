using CommunityToolkit.Mvvm.ComponentModel;

namespace unreal_GUI.ViewModel
{
    public partial class UserControlViewModel : ObservableObject
    {
        // 矩形的位置和尺寸属性
        [ObservableProperty]
        private double rectLeft;

        [ObservableProperty]
        private double rectTop;

        [ObservableProperty]
        private double rectWidth = 300;

        [ObservableProperty]
        private double rectHeight = 100;

        // 边缘热区的位置和尺寸属性
        [ObservableProperty]
        private double edgeTopLeft;

        [ObservableProperty]
        private double edgeTopTop;

        [ObservableProperty]
        private double edgeTopWidth;

        [ObservableProperty]
        private double edgeTopHeight = 8;

        [ObservableProperty]
        private double edgeBottomLeft;

        [ObservableProperty]
        private double edgeBottomTop;

        [ObservableProperty]
        private double edgeBottomWidth;

        [ObservableProperty]
        private double edgeBottomHeight = 8;

        [ObservableProperty]
        private double edgeLeftLeft;

        [ObservableProperty]
        private double edgeLeftTop;

        [ObservableProperty]
        private double edgeLeftWidth = 8;

        [ObservableProperty]
        private double edgeLeftHeight;

        [ObservableProperty]
        private double edgeRightLeft;

        [ObservableProperty]
        private double edgeRightTop;

        [ObservableProperty]
        private double edgeRightWidth = 8;

        [ObservableProperty]
        private double edgeRightHeight;

        // 拖拽相关属性
        [ObservableProperty]
        private bool isDragging = false;

        [ObservableProperty]
        private string? dragEdge = null;

        [ObservableProperty]
        private double clickPositionX;

        [ObservableProperty]
        private double clickPositionY;

        // 画布尺寸
        private double canvasWidth;
        private double canvasHeight;

        public UserControlViewModel()
        {
            // 初始化时居中显示矩形
            InitializeRectPosition();
        }

        public void SetCanvasSize(double width, double height)
        {
            canvasWidth = width;
            canvasHeight = height;
            InitializeRectPosition();
        }

        private void InitializeRectPosition()
        {
            if (canvasWidth > 0 && canvasHeight > 0)
            {
                RectLeft = (canvasWidth - RectWidth) / 2;
                RectTop = (canvasHeight - RectHeight) / 2;
                UpdateEdgeRects();
            }
        }

        public void UpdateEdgeRects()
        {
            double edgeThickness = 8; // 热区厚度

            // 顶部
            EdgeTopWidth = RectWidth;
            EdgeTopHeight = edgeThickness;
            EdgeTopLeft = RectLeft;
            EdgeTopTop = RectTop - edgeThickness / 2;

            // 底部
            EdgeBottomWidth = RectWidth;
            EdgeBottomHeight = edgeThickness;
            EdgeBottomLeft = RectLeft;
            EdgeBottomTop = RectTop + RectHeight - edgeThickness / 2;

            // 左侧
            EdgeLeftWidth = edgeThickness;
            EdgeLeftHeight = RectHeight;
            EdgeLeftLeft = RectLeft - edgeThickness / 2;
            EdgeLeftTop = RectTop;

            // 右侧
            EdgeRightWidth = edgeThickness;
            EdgeRightHeight = RectHeight;
            EdgeRightLeft = RectLeft + RectWidth - edgeThickness / 2;
            EdgeRightTop = RectTop;
        }

        // 开始拖拽边缘
        public void StartEdgeDrag(double posX, double posY, string edgeName)
        {
            IsDragging = true;
            DragEdge = edgeName;
            ClickPositionX = posX;
            ClickPositionY = posY;
        }

        // 拖拽边缘调整大小
        public void ResizeEdge(double posX, double posY, string edgeName)
        {
            if (!IsDragging || string.IsNullOrEmpty(DragEdge))
                return;

            double deltaX = posX - ClickPositionX;
            double deltaY = posY - ClickPositionY;

            // 根据边缘名称决定调整方向
            switch (edgeName)
            {
                case "EdgeTop":
                    ResizeFromTop(deltaY);
                    break;
                case "EdgeBottom":
                    ResizeFromBottom(deltaY);
                    break;
                case "EdgeLeft":
                    ResizeFromLeft(deltaX);
                    break;
                case "EdgeRight":
                    ResizeFromRight(deltaX);
                    break;
            }

            ClickPositionX = posX;
            ClickPositionY = posY;
        }

        private void ResizeFromTop(double deltaY)
        {
            double newHeight = RectHeight - deltaY;
            double newWidth = newHeight * 3.0;
            double newTop = RectTop + (RectHeight - newHeight);
            double newLeft = RectLeft + (RectWidth - newWidth) / 2;

            // 边界检查
            if (newHeight > 0 && newWidth > 0 && 
                newTop >= 0 && newLeft >= 0 && 
                newTop + newHeight <= canvasHeight && 
                newLeft + newWidth <= canvasWidth)
            {
                RectTop = newTop;
                RectLeft = newLeft;
                RectHeight = newHeight;
                RectWidth = newWidth;
                UpdateEdgeRects();
            }
        }

        private void ResizeFromBottom(double deltaY)
        {
            double newHeight = RectHeight + deltaY;
            double newWidth = newHeight * 3.0;
            double newLeft = RectLeft + (RectWidth - newWidth) / 2;

            // 边界检查
            if (newHeight > 0 && newWidth > 0 && 
                RectTop >= 0 && newLeft >= 0 && 
                RectTop + newHeight <= canvasHeight && 
                newLeft + newWidth <= canvasWidth)
            {
                RectHeight = newHeight;
                RectWidth = newWidth;
                RectLeft = newLeft;
                UpdateEdgeRects();
            }
        }

        private void ResizeFromLeft(double deltaX)
        {
            double newWidth = RectWidth - deltaX;
            double newHeight = newWidth / 3.0;
            double newLeft = RectLeft + (RectWidth - newWidth);
            double newTop = RectTop + (RectHeight - newHeight) / 2;

            // 边界检查
            if (newWidth > 0 && newHeight > 0 && 
                newTop >= 0 && newLeft >= 0 && 
                newTop + newHeight <= canvasHeight && 
                newLeft + newWidth <= canvasWidth)
            {
                RectLeft = newLeft;
                RectTop = newTop;
                RectWidth = newWidth;
                RectHeight = newHeight;
                UpdateEdgeRects();
            }
        }

        private void ResizeFromRight(double deltaX)
        {
            double newWidth = RectWidth + deltaX;
            double newHeight = newWidth / 3.0;
            double newTop = RectTop + (RectHeight - newHeight) / 2;

            // 边界检查
            if (newWidth > 0 && newHeight > 0 && 
                newTop >= 0 && RectLeft >= 0 && 
                newTop + newHeight <= canvasHeight && 
                RectLeft + newWidth <= canvasWidth)
            {
                RectWidth = newWidth;
                RectHeight = newHeight;
                RectTop = newTop;
                UpdateEdgeRects();
            }
        }

        // 结束拖拽
        public void EndDrag()
        {
            IsDragging = false;
            DragEdge = null;
        }

        // 开始移动整个矩形
        public void StartMove(double posX, double posY)
        {
            IsDragging = true;
            DragEdge = "Move";
            ClickPositionX = posX;
            ClickPositionY = posY;
        }

        // 移动整个矩形
        public void MoveRect(double posX, double posY)
        {
            if (IsDragging && DragEdge == "Move")
            {
                double offsetX = posX - ClickPositionX;
                double offsetY = posY - ClickPositionY;

                double newLeft = RectLeft + offsetX;
                double newTop = RectTop + offsetY;

                // 边界检查
                if (newLeft >= 0 && newTop >= 0 && 
                    newLeft + RectWidth <= canvasWidth && 
                    newTop + RectHeight <= canvasHeight)
                {
                    RectLeft = newLeft;
                    RectTop = newTop;
                    UpdateEdgeRects();

                    ClickPositionX = posX;
                    ClickPositionY = posY;
                }
            }
        }
    }
}