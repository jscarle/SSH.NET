﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Renci.SshNet.Sftp.Messages;
using System.Globalization;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Represents SFTP file information
    /// </summary>
    public class SftpFile
    {
        #region Bitmask constats

        private static UInt32 S_IFMT = 0xF000;  //  bitmask for the file type bitfields

        private static UInt32 S_IFSOCK = 0xC000;  //	socket

        private static UInt32 S_IFLNK = 0xA000;  //	symbolic link

        private static UInt32 S_IFREG = 0x8000;  //	regular file

        private static UInt32 S_IFBLK = 0x6000;  //	block device

        private static UInt32 S_IFDIR = 0x4000;  //	directory

        private static UInt32 S_IFCHR = 0x2000;  //	character device

        private static UInt32 S_IFIFO = 0x1000;  //	FIFO

        private static UInt32 S_ISUID = 0x0800;  //	set UID bit

        private static UInt32 S_ISGID = 0x0400;  //	set-group-ID bit (see below)

        private static UInt32 S_ISVTX = 0x0200;  //	sticky bit (see below)

        private static UInt32 S_IRUSR = 0x0100;  //	owner has read permission

        private static UInt32 S_IWUSR = 0x0080;  //	owner has write permission

        private static UInt32 S_IXUSR = 0x0040;  //	owner has execute permission

        private static UInt32 S_IRGRP = 0x0020;  //	group has read permission

        private static UInt32 S_IWGRP = 0x0010;  //	group has write permission

        private static UInt32 S_IXGRP = 0x0008;  //	group has execute permission

        private static UInt32 S_IROTH = 0x0004;  //	others have read permission

        private static UInt32 S_IWOTH = 0x0002;  //	others have write permission

        private static UInt32 S_IXOTH = 0x0001;  //	others have execute permission

        #endregion

        private bool _isBitFiledsBitSet;
        private bool _isUIDBitSet;
        private bool _isGroupIDBitSet;
        private bool _isStickyBitSet;

        private SftpSession _sftpSession;

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpFile"/> class.
        /// </summary>
        /// <param name="sftpSession">The SFTP session.</param>
        /// <param name="fullName">Full path of the directory or file.</param>
        /// <param name="attributes">Attributes of the directory or file.</param>
        internal SftpFile(SftpSession sftpSession, string fullName, SftpFileAttributes attributes)
        {
            this._sftpSession = sftpSession;

            this.Name = fullName.Substring(fullName.LastIndexOf('/') + 1);

            this.FullName = fullName;

            if (attributes.AccessTime.HasValue)
                this.LastAccessTime = attributes.AccessTime.Value;
            else
                this.LastAccessTime = DateTime.MinValue;

            if (attributes.ModifyTime.HasValue)
                this.LastWriteTime = attributes.ModifyTime.Value;
            else
                this.LastWriteTime = DateTime.MinValue;

            if (attributes.Size.HasValue)
                this.Size = (long)attributes.Size.Value;
            else
                this.Size = -1;

            if (attributes.UserId.HasValue)
                this.UserId = (int)attributes.UserId.Value;
            else
                this.UserId = -1;

            if (attributes.GroupId.HasValue)
                this.GroupId = (int)attributes.GroupId.Value;
            else
                this.GroupId = -1;

            this._isBitFiledsBitSet = ((attributes.Permissions & S_IFMT) == S_IFMT);

            this.IsSocket = ((attributes.Permissions & S_IFSOCK) == S_IFSOCK);

            this.IsSymbolicLink = ((attributes.Permissions & S_IFLNK) == S_IFLNK);

            this.IsRegularFile = ((attributes.Permissions & S_IFREG) == S_IFREG);

            this.IsBlockDevice = ((attributes.Permissions & S_IFBLK) == S_IFBLK);

            this.IsDirectory = ((attributes.Permissions & S_IFDIR) == S_IFDIR);

            this.IsCharacterDevice = ((attributes.Permissions & S_IFCHR) == S_IFCHR);

            this.IsNamedPipe = ((attributes.Permissions & S_IFIFO) == S_IFIFO);

            this._isUIDBitSet = ((attributes.Permissions & S_ISUID) == S_ISUID);

            this._isGroupIDBitSet = ((attributes.Permissions & S_ISGID) == S_ISGID);

            this._isStickyBitSet = ((attributes.Permissions & S_ISVTX) == S_ISVTX);

            this.OwnerCanRead = ((attributes.Permissions & S_IRUSR) == S_IRUSR);

            this.OwnerCanWrite = ((attributes.Permissions & S_IWUSR) == S_IWUSR);

            this.OwnerCanExecute = ((attributes.Permissions & S_IXUSR) == S_IXUSR);

            this.GroupCanRead = ((attributes.Permissions & S_IRGRP) == S_IRGRP);

            this.GroupCanWrite = ((attributes.Permissions & S_IWGRP) == S_IWGRP);

            this.GroupCanExecute = ((attributes.Permissions & S_IXGRP) == S_IXGRP);

            this.OthersCanRead = ((attributes.Permissions & S_IROTH) == S_IROTH);

            this.OthersCanWrite = ((attributes.Permissions & S_IWOTH) == S_IWOTH);

            this.OthersCanExecute = ((attributes.Permissions & S_IXOTH) == S_IXOTH);

            this.Extensions = attributes.Extensions;
        }

        /// <summary>
        /// Gets the full path of the directory or file.
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// For files, gets the name of the file. For directories, gets the name of the last directory in the hierarchy if a hierarchy exists. 
        /// Otherwise, the Name property gets the name of the directory.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the time the current file or directory was last accessed.
        /// </summary>
        /// <value>
        /// The time that the current file or directory was last accessed.
        /// </value>
        public DateTime LastAccessTime { get; private set; }

        /// <summary>
        /// Gets or sets the time when the current file or directory was last written to.
        /// </summary>
        /// <value>
        /// The time the current file was last written.
        /// </value>
        public DateTime LastWriteTime { get; private set; }

        /// <summary>
        /// Gets or sets the size, in bytes, of the current file.
        /// </summary>
        /// <value>
        /// The size of the current file in bytes.
        /// </value>
        public long Size { get; private set; }

        /// <summary>
        /// Gets or sets file user id.
        /// </summary>
        /// <value>
        /// File user id.
        /// </value>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets file group id.
        /// </summary>
        /// <value>
        /// File group id.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets a value indicating whether file represents a socket.
        /// </summary>
        /// <value>
        ///   <c>true</c> if file represents a socket; otherwise, <c>false</c>.
        /// </value>
        public bool IsSocket { get; private set; }

        /// <summary>
        /// Gets a value indicating whether file represents a symbolic link.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if file represents a symbolic link; otherwise, <c>false</c>.
        /// </value>
        public bool IsSymbolicLink { get; private set; }

        /// <summary>
        /// Gets a value indicating whether file represents a regular file.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if file represents a regular file; otherwise, <c>false</c>.
        /// </value>
        public bool IsRegularFile { get; private set; }

        /// <summary>
        /// Gets a value indicating whether file represents a block device.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if file represents a block device; otherwise, <c>false</c>.
        /// </value>
        public bool IsBlockDevice { get; private set; }

        /// <summary>
        /// Gets a value indicating whether file represents a directory.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if file represents a directory; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirectory { get; private set; }

        /// <summary>
        /// Gets a value indicating whether file represents a character device.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if file represents a character device; otherwise, <c>false</c>.
        /// </value>
        public bool IsCharacterDevice { get; private set; }

        /// <summary>
        /// Gets a value indicating whether file represents a named pipe.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if file represents a named pipe; otherwise, <c>false</c>.
        /// </value>
        public bool IsNamedPipe { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the owner can read from this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if owner can read from this file; otherwise, <c>false</c>.
        /// </value>
        public bool OwnerCanRead { get; set; }

        /// <summary>
        /// Gets a value indicating whether the owner can write into this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if owner can write into this file; otherwise, <c>false</c>.
        /// </value>
        public bool OwnerCanWrite { get; set; }

        /// <summary>
        /// Gets a value indicating whether the owner can execute this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if owner can execute this file; otherwise, <c>false</c>.
        /// </value>
        public bool OwnerCanExecute { get; set; }

        /// <summary>
        /// Gets a value indicating whether the group members can read from this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if group members can read from this file; otherwise, <c>false</c>.
        /// </value>
        public bool GroupCanRead { get; set; }

        /// <summary>
        /// Gets a value indicating whether the group members can write into this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if group members can write into this file; otherwise, <c>false</c>.
        /// </value>
        public bool GroupCanWrite { get; set; }

        /// <summary>
        /// Gets a value indicating whether the group members can execute this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if group members can execute this file; otherwise, <c>false</c>.
        /// </value>
        public bool GroupCanExecute { get; set; }

        /// <summary>
        /// Gets a value indicating whether the others can read from this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if others can read from this file; otherwise, <c>false</c>.
        /// </value>
        public bool OthersCanRead { get; set; }

        /// <summary>
        /// Gets a value indicating whether the others can write into this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if others can write into this file; otherwise, <c>false</c>.
        /// </value>
        public bool OthersCanWrite { get; set; }

        /// <summary>
        /// Gets a value indicating whether the others can execute this file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if others can execute this file; otherwise, <c>false</c>.
        /// </value>
        public bool OthersCanExecute { get; set; }

        /// <summary>
        /// Gets the extension part of the file.
        /// </summary>
        /// <value>
        /// File extensions.
        /// </value>
        public IDictionary<string, string> Extensions { get; private set; }

        /// <summary>
        /// Updates file status on the server.
        /// </summary>
        public void UpdateStatus()
        {
            using (var setCmd = new SetFileStatusCommand(this._sftpSession, this.FullName, this.GetAttributes()))
            {
                setCmd.CommandTimeout = TimeSpan.FromSeconds(30);

                setCmd.Execute();
            }
        }

        /// <summary>
        /// Sets file  permissions.
        /// </summary>
        /// <param name="mode">The mode.</param>
        public void SetPermissions(ushort mode)
        {
            var modeBytes = mode.ToString(CultureInfo.InvariantCulture).ToArray();

            var permission = (modeBytes[0] & 0x0F) * 8 * 8 + (modeBytes[1] & 0x0F) * 8 + (modeBytes[2] & 0x0F);


            this.OwnerCanRead = (permission & S_IRUSR) == S_IRUSR;
            this.OwnerCanWrite = (permission & S_IWUSR) == S_IWUSR;
            this.OwnerCanExecute = (permission & S_IXUSR) == S_IXUSR;

            this.GroupCanRead = (permission & S_IRGRP) == S_IRGRP;
            this.GroupCanWrite = (permission & S_IWGRP) == S_IWGRP;
            this.GroupCanExecute = (permission & S_IXGRP) == S_IXGRP;

            this.OthersCanRead = (permission & S_IROTH) == S_IROTH;
            this.OthersCanWrite = (permission & S_IWOTH) == S_IWOTH;
            this.OthersCanExecute = (permission & S_IXOTH) == S_IXOTH;

            this.UpdateStatus();
        }

        /// <summary>
        /// Permanently deletes a file on remote machine.
        /// </summary>
        [SecuritySafeCritical]
        public void Delete()
        {
            SftpCommand cmd = null;
            try
            {
                if (this.IsDirectory)
                {
                    cmd = new RemoveDirectoryCommand(this._sftpSession, this.FullName);
                }
                else
                {
                    cmd = new RemoveFileCommand(this._sftpSession, this.FullName);
                }

                cmd.CommandTimeout = TimeSpan.FromSeconds(30);

                cmd.Execute();
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose();
                }
            }
        }

        /// <summary>
        /// Moves a specified file to a new location on remote machine, providing the option to specify a new file name.
        /// </summary>
        /// <param name="destFileName">The path to move the file to, which can specify a different file name.</param>
        [SecuritySafeCritical]
        public void MoveTo(string destFileName)
        {
            using (var setCmd = new RenameFileCommand(this._sftpSession, this.FullName, destFileName))
            {
                setCmd.CommandTimeout = TimeSpan.FromSeconds(30);

                setCmd.Execute();
            }
        }

        /// <summary>
        /// Gets the file attributes.
        /// </summary>
        /// <returns></returns>
        private SftpFileAttributes GetAttributes()
        {
            var attributes = new SftpFileAttributes();

            //  Populate only updateable attributes
            if (this.UserId > -1)
            {
                attributes.UserId = new Nullable<uint>((uint)this.UserId);
            }
            else
            {
                attributes.UserId = null;
            }

            if (this.GroupId > -1)
            {
                attributes.GroupId = new Nullable<uint>((uint)this.GroupId);
            }
            else
            {
                attributes.GroupId = null;
            }

            UInt32 permission = 0;

            if (this._isBitFiledsBitSet)
                permission = permission | S_IFMT;

            if (this.IsSocket)
                permission = permission | S_IFSOCK;

            if (this.IsSymbolicLink)
                permission = permission | S_IFLNK;

            if (this.IsRegularFile)
                permission = permission | S_IFREG;

            if (this.IsBlockDevice)
                permission = permission | S_IFBLK;

            if (this.IsDirectory)
                permission = permission | S_IFDIR;

            if (this.IsCharacterDevice)
                permission = permission | S_IFCHR;

            if (this.IsNamedPipe)
                permission = permission | S_IFIFO;

            if (this._isUIDBitSet)
                permission = permission | S_ISUID;

            if (this._isGroupIDBitSet)
                permission = permission | S_ISGID;

            if (this._isStickyBitSet)
                permission = permission | S_ISVTX;

            if (this.OwnerCanRead)
                permission = permission | S_IRUSR;

            if (this.OwnerCanWrite)
                permission = permission | S_IWUSR;

            if (this.OwnerCanExecute)
                permission = permission | S_IXUSR;

            if (this.GroupCanRead)
                permission = permission | S_IRGRP;

            if (this.GroupCanWrite)
                permission = permission | S_IWGRP;

            if (this.GroupCanExecute)
                permission = permission | S_IXGRP;

            if (this.OthersCanRead)
                permission = permission | S_IROTH;

            if (this.OthersCanWrite)
                permission = permission | S_IWOTH;

            if (this.OthersCanExecute)
                permission = permission | S_IXOTH;

            attributes.Permissions = permission;

            return attributes;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "Name {0}, Size {1}, User ID {2}, Group ID {3}, Accessed {4}, Modified {5}", this.Name, this.Size, this.UserId, this.GroupId, this.LastAccessTime, this.LastWriteTime);
        }
    }
}