// Form.DragDrop.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License. 
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//


namespace Ionic.Zip.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using DragDropLib;
    using ComIDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

    public partial class ZipForm : System.Windows.Forms.Form
    {
        partial void SetDragDrop()
        {
            this.listView2.DragDrop += new System.Windows.Forms.DragEventHandler(this.control_OnDragDrop);
            this.listView2.DragEnter += new System.Windows.Forms.DragEventHandler(this.control_OnDragEnter);
            this.listView2.DragOver += new System.Windows.Forms.DragEventHandler(this.control_OnDragOver);
            this.listView2.DragLeave += new System.EventHandler(this.control_OnDragLeave);

            this.listView1.DragDrop += new System.Windows.Forms.DragEventHandler(this.control_OnDragDrop);
            this.listView1.DragEnter += new System.Windows.Forms.DragEventHandler(this.control_OnDragEnter);
            this.listView1.DragOver += new System.Windows.Forms.DragEventHandler(this.control_OnDragOver);
            this.listView1.DragLeave += new System.EventHandler(this.control_OnDragLeave);
        }

        protected void control_OnDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
            Point p = Cursor.Position;
            Win32Point wp;
            wp.x = p.X;
            wp.y = p.Y;
            IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
            dropHelper.DragEnter(IntPtr.Zero, (ComIDataObject)e.Data, ref wp, (int)e.Effect);
        }

        protected void control_OnDragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
            Point p = Cursor.Position;
            Win32Point wp;
            wp.x = p.X;
            wp.y = p.Y;
            IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
            dropHelper.DragOver(ref wp, (int)e.Effect);
        }

        protected void control_OnDragLeave(object sender, EventArgs e)
        {
            IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
            dropHelper.DragLeave();
        }

        protected void control_OnDragDrop(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
            Point p = Cursor.Position;
            Win32Point wp;
            wp.x = p.X;
            wp.y = p.Y;
            IDropTargetHelper dropHelper = (IDropTargetHelper)new DragDropHelper();
            dropHelper.Drop((ComIDataObject)e.Data, ref wp, (int)e.Effect);
        }

    }

}





namespace DragDropLib
{

    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Collections.Generic;

    [ComImport]
    [Guid("4657278A-411B-11d2-839A-00C04FD918D0")]
    public class DragDropHelper { }



    [ComVisible(true)]
    [ComImport]
    [Guid("4657278B-411B-11D2-839A-00C04FD918D0")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDropTargetHelper
    {
        void DragEnter(
                   [In] IntPtr hwndTarget,
                   [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject,
                   [In] ref Win32Point pt,
                   [In] int effect);

        void DragLeave();

        void DragOver(
                  [In] ref Win32Point pt,
                  [In] int effect);

        void Drop(
                      [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject,
                  [In] ref Win32Point pt,
                  [In] int effect);

        void Show(
                      [In] bool show);
    }



    [StructLayout(LayoutKind.Sequential)]
    public struct Win32Point
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Win32Size
    {
        public int cx;
        public int cy;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ShDragImage
    {
        public Win32Size sizeDragImage;
        public Win32Point ptOffset;
        public IntPtr hbmpDragImage;
        public int crColorKey;
    }



    [ComVisible(true)]
    [ComImport]
    [Guid("DE5BF786-477A-11D2-839D-00C04FD918D0")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IDragSourceHelper
    {
        void InitializeFromBitmap(
            [In, MarshalAs(UnmanagedType.Struct)] ref ShDragImage dragImage,
            [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject);

        void InitializeFromWindow(
            [In] IntPtr hwnd,
            [In] ref Win32Point pt,
            [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject);
    }

}


