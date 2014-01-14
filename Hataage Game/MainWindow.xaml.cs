//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;
    //using System.Drawing.Point;
    using System.Windows.Forms;
    //using System.Drawing;
    //using System.Windows.Forms;
    //using System;


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        private float dot,dot2;

        //ランダムを宣言
        //private Random randm;
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            Image.Source = this.imageSource;

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }
                using (DrawingContext dc = this.drawingGroup.Open())
                {

                    //ここにゲーム要素を追加

                    //ランダムな値を0から4まで用意
                    //System.Random r = new System.Random(4);

                    int c1 = 0;
                    int c2 = 0;
                    int c3 = 0;
                    int c4 = 0;

                    //for文で5(0,1,2,3,4)回繰り返す
                    //for(int a=0; a<5; a++ ){

                      //b1の値をランダムに0から4まで設定
                      //int b1 = r.Next(0,4);
                        if (c1 == 0)
                        {
                            Label.Content = "右上げて";
                            Label.FontSize = 20;
                            //正解もしくは不正解して手を下げたらループを抜けるようにする
                            do
                            {
                                Label1.Content = "その手は";
                                Label1.FontSize = 20;
                                //左手を上げたら
                                if (this.dot > 0.5f)
                                {
                                    Label1.Content = "不正解";
                                    Label.FontSize = 20;
                                    c2 = c2 + 1;

                                }
                                //右手を上げたら
                                else if (this.dot2 > 0.5f)
                                {
                                    Label1.Content = "正解";
                                    Label.FontSize = 20;
                                    c3 = c3 + 1;
                                    //正解したら、手を下げるようにする
                                    /*if(c3==1){
                                     　　do{
                                              if(this.dot < 0.5f){
                                                 
                                     */

                                }
                         　 //手を下げたら
                            }while (c4==1);
                        }
                        /*if (c1 == 1)
                        {
                            Label.Content = "左上げて";
                            Label.FontSize = 20;
                            do
                            {
                                if (this.dot > 0.5f)
                                {
                                    Label1.Content = "正解";
                                    Label.FontSize = 20;
                                    break;
                                }
                                else if (this.dot2 > 0.5f)
                                {
                                    Label1.Content = "不正解";
                                    Label.FontSize = 20;
                                    break;
                                }
                            }while (c2 == 1);
                        }


                    
                        /*if (c1 == 1){
                            Label.Content = "左上げて";
                            Label.FontSize = 20;
                        }

                        if (b1 == 2){
                            Label.Content = "右げないで左上げて";
                            Label.FontSize = 20;
                        }

                        if (b1 == 3){
                            Label.Content = "左げないで右上げて";
                            Label.FontSize = 20;
                        }
                   }*/





                    // Draw a transparent background to set the render size                
                    dc.DrawRectangle(Brushes.White, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));


                    if (skeletons.Length != 0)
                    {
                        foreach (Skeleton skel in skeletons)
                        {
                            RenderClippedEdges(skel, dc);


                            if (skel.TrackingState == SkeletonTrackingState.Tracked)
                            {
                                this.DrawBonesAndJoints(skel, dc);

                                //右
                                Vector4 vec1, vec2;
                                vec1 = new Vector4();
                                vec2 = new Vector4();
                                vec1.X = skel.Joints[JointType.ElbowLeft].Position.X - skel.Joints[JointType.ShoulderLeft].Position.X;
                                vec1.Y = skel.Joints[JointType.ElbowLeft].Position.Y - skel.Joints[JointType.ShoulderLeft].Position.Y;
                                vec1.Z = skel.Joints[JointType.ElbowLeft].Position.Z - skel.Joints[JointType.ShoulderLeft].Position.Z;

                                vec2.X = skel.Joints[JointType.ElbowLeft].Position.X - skel.Joints[JointType.HandLeft].Position.X;
                                vec2.Y = skel.Joints[JointType.ElbowLeft].Position.Y - skel.Joints[JointType.HandLeft].Position.Y;
                                vec2.Z = skel.Joints[JointType.ElbowLeft].Position.Z - skel.Joints[JointType.HandLeft].Position.Z;

                                float AA, BB, AB;
                                AA = vec1.X * vec1.X + vec1.Y * vec1.Y + vec1.Z * vec1.Z;
                                BB = vec2.X * vec2.X + vec2.Y * vec2.Y + vec2.Z * vec2.Z;
                                AB = vec1.X * vec2.X + vec1.Y * vec2.Y + vec1.Z * vec2.Z;
                                dot = (float)(AB / (System.Math.Sqrt(AA) * System.Math.Sqrt(BB)));


                                //左
                                Vector4 vec3, vec4;
                                vec3 = new Vector4();
                                vec4 = new Vector4();

                                vec3.X = skel.Joints[JointType.ElbowRight].Position.X - skel.Joints[JointType.ShoulderRight].Position.X;
                                vec3.Y = skel.Joints[JointType.ElbowRight].Position.Y - skel.Joints[JointType.ShoulderRight].Position.Y;
                                vec3.Z = skel.Joints[JointType.ElbowRight].Position.Z - skel.Joints[JointType.ShoulderRight].Position.Z;

                                vec4.X = skel.Joints[JointType.ElbowRight].Position.X - skel.Joints[JointType.HandRight].Position.X;
                                vec4.Y = skel.Joints[JointType.ElbowRight].Position.Y - skel.Joints[JointType.HandRight].Position.Y;
                                vec4.Z = skel.Joints[JointType.ElbowRight].Position.Z - skel.Joints[JointType.HandRight].Position.Z;

                                float AA2, BB2, AB2;

                                AA2 = vec3.X * vec3.X + vec3.Y * vec3.Y + vec3.Z * vec3.Z;
                                BB2 = vec4.X * vec4.X + vec4.Y * vec4.Y + vec4.Z * vec4.Z;
                                AB2 = vec3.X * vec4.X + vec3.Y * vec4.Y + vec3.Z * vec4.Z;
                                dot2 = (float)(AB2 / (System.Math.Sqrt(AA2) * System.Math.Sqrt(BB2)));





                                //Label.Content = skel.Joints[JointType.Spine].Position.Z;
                            }
                            else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                            {
                                dc.DrawEllipse(
                                this.centerPointBrush,
                                null,
                                this.SkeletonPointToScreen(skel.Position),
                                BodyCenterThickness,
                                BodyCenterThickness);
                            }
                        }
                    }

                    // prevent drawing outside of our render area
                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                }
        }

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);



            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
        {
            if (null != this.sensor)
            {
                if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                }
                else
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                }
            }
        }
        
    }
}