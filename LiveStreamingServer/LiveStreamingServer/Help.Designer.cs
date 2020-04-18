﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LiveStreamingServer {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Help {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Help() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LiveStreamingServer.Help", typeof(Help).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Usage:
        ///	LiveStreamingServer.exe -ffmpeg &lt;path&gt; -output &lt;path&gt; \[-host &lt;name&gt;\] \[-ffmpegport &lt;port:8889&gt;\] \[-fps &lt;fps&gt;\] \[-keyframe &lt;frames&gt;\] \[-listsize &lt;size&gt;\] \[-hlswrap &lt;count&gt;\]
        ///
        ///Options:
        ///	--ffmpeg, -f		Full path to ffmpeg.exe.
        ///	--output, -o		Full path to file output.
        ///	--host, -d		Hostname for web server.
        ///	--ffmpegport, -p	Port for ffmpeg server. Default 8889
        ///	--fps, -r		Target FPS output. Default 30
        ///	--keyframe, -k		Frames until key frame. This has a large effect on the stream delay. The  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Message {
            get {
                return ResourceManager.GetString("Message", resourceCulture);
            }
        }
    }
}
