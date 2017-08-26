using System;
using System.ComponentModel;
using System.Text;
using Deezy.Cryptography.Engines;
using Deezy.Cryptography.Paddings;
using Deezy.Cryptography.Parameters;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Android.Content;
using Android.OS;
using Android.Provider;
using Uri = Android.Net.Uri;
using Android.Database;
using Android.Util;

namespace Warpten.Utils
{
    public static class UriExtensions
    {
        public static string ToPhysicalPath(this Uri uri, Context ctx)
        {
            var isKitkat = Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;

            var isDocumentUri = DocumentsContract.IsDocumentUri(ctx, uri);
            var isTreeUri = DocumentsContract.IsTreeUri(uri);

            if (isKitkat && (isDocumentUri || isTreeUri))
            {
                var rootUri = isDocumentUri
                    ? DocumentsContract.GetDocumentId(uri)
                    : DocumentsContract.GetTreeDocumentId(uri);

                if (uri.Authority == "com.android.localstorage.documents")
                    return rootUri;
                
                if (uri.Authority == "com.android.externalstorage.documents")
                {
                    var splitDocumentId = rootUri.Split(':');
                    var type = splitDocumentId[0];

                    if (string.Compare(type, "primary", StringComparison.InvariantCultureIgnoreCase) == 0)
                        return Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, splitDocumentId[1]);

                    // Handle non-primary
                    //! TODO: This is absolutely disgusting but android offers no easy way to obtain a path to a directory from an Uri to a directory.
                    //! I'm not even sure this is portable.
                    var contentUri = MediaStore.Files.GetContentUri(type);
                    var cursor = ctx.ContentResolver.Query(contentUri, null, null, null, null);
                    if (cursor != null && cursor.MoveToFirst())
                    {
                        var path = cursor.GetString(0);
                        cursor.Close();
                        return path;
                    }
                    else
                    {
                        return "/storage/" + rootUri.Replace(':', '/');
                    }

                    return contentUri.ToPhysicalPath(ctx);
                }

                if (uri.Authority == "com.android.providers.downloads.documents")
                {
                    var contentUri = ContentUris.WithAppendedId(Uri.Parse("content://downloads/public_downloads"), long.Parse(rootUri));

                    return ctx.GetDataColumn(contentUri, MediaStore.MediaColumns.Data, null, null);
                }

                if (uri.Authority == "com.android.providers.media.documents")
                {
                    var splitDocumentId = rootUri.Split(':');
                    var type = splitDocumentId[0];

                    Uri contentUri = null;

                    if ("image" == type)
                        contentUri = MediaStore.Images.Media.ExternalContentUri;
                    else if ("video" == type)
                        contentUri = MediaStore.Video.Media.ExternalContentUri;
                    else if ("audio" == type)
                        contentUri = MediaStore.Audio.Media.ExternalContentUri;

                    return ctx.GetDataColumn(contentUri, MediaStore.MediaColumns.Data, "_id?=", splitDocumentId[1]);
                }
            }
            // MediaStore and general
            else if (uri.Scheme == "content")
            {
                if (uri.Authority == "com.google.android.apps.photos.content")
                    return uri.LastPathSegment;

                return ctx.GetDataColumn(uri, MediaStore.MediaColumns.Data, null, null);
            }
            else if (uri.Scheme == "file")
                return uri.Path;

            return "";
        }

        public static string GetDataColumn(this Context ctx, Uri uri, string projection, string selection,
            params string[] selectionArguments)
        {
            ICursor cursor = null;

            try
            {
                cursor = ctx.ContentResolver.Query(uri, new[] { projection }, selection, selectionArguments, null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    var columnIndex = cursor.GetColumnIndexOrThrow(MediaStore.MediaColumns.Data);
                    return cursor.GetString(columnIndex);
                }
            }
            catch (Exception e)
            {
                
            }
            cursor?.Close();
            return null;
        }
    }
}
