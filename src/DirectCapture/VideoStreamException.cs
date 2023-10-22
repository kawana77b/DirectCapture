using System.Runtime.InteropServices;

/* **************************************************************
 *
 * This code is provided as is, without any warranty.
 * The entire risk as to the quality, the performance of the code
 * for any particular purpose lies with the party using the code.
 *
 * You may edit it and use into your programs, but if you do so,
 * remember to put this text into the credits window and/or into
 * documentation (if applicable)
 * "This program uses code developed by Jocker.
 * http://www.jockersoft.com"
 *
 ****************************************************************/

namespace DirectCapture
{
    /// <summary>
    /// This is a wrapper exception for COMException regarding video streams.
    /// </summary>
    public class VideoStreamException : COMException
    {
        private readonly COMException _error;

        /// <summary>
        /// Error Code
        /// </summary>
        public override int ErrorCode => _error.ErrorCode;

        /// <summary>
        /// Get error message
        /// </summary>
        public override string Message
        {
            get
            {
                // If there is a message, it takes precedence. If it does not exist, the template is used.
                var msg = string.IsNullOrEmpty(_error.Message) ? GetMessage() : _error.Message;
                return $"code:{ErrorCode} {msg}";
            }
        }

        /// <summary>
        /// Generate a new instance
        /// </summary>
        /// <param name="error"></param>
        public VideoStreamException(COMException error)
        {
            _error = error;
        }

        /// <summary>
        /// Get error message from error code
        /// </summary>
        /// <returns></returns>
        private string GetMessage()
        {
            switch ((uint)ErrorCode)
            {
                case 0x80040200:	//VFW_E_INVALIDMEDIATYPE
                    return "An invalid media type was specified";

                case 0x80040201:	//VFW_E_INVALIDSUBTYPE
                    return "An invalid media subtype was specified";

                case 0x80040202:	//VFW_E_NEED_OWNER
                    return "This object can only be created as an aggregated object";

                case 0x80040203:	//VFW_E_ENUM_OUT_OF_SYNC
                    return "The enumerator has become invalid";

                case 0x80040204:	//VFW_E_ALREADY_CONNECTED
                    return "At least one of the pins involved in the operation is already connected";

                case 0x80040205:	//VFW_E_FILTER_ACTIVE
                    return "This operation cannot be performed because the filter is active";

                case 0x80040206:	//VFW_E_NO_TYPES
                    return "One of the specified pins supports no media types";

                case 0x80040207:	//VFW_E_NO_ACCEPTABLE_TYPES
                    return "There is no common media type between these pins";

                case 0x80040208:	//VFW_E_INVALID_DIRECTION
                    return "Two pins of the same direction cannot be connected together";

                case 0x80040209:	//VFW_E_NOT_CONNECTED
                    return "The operation cannot be performed because the pins are not connected";

                case 0x80040210:	//VFW_E_NO_ALLOCATOR
                    return "No sample buffer allocator is available";

                case 0x80040211:	//VFW_E_NOT_COMMITTED
                    return "Cannot allocate a sample when the allocator is not active";

                case 0x80040212:	//VFW_E_SIZENOTSET
                    return "Cannot allocate memory because no size has been set";

                case 0x80040213:	//VFW_E_NO_CLOCK
                    return "Cannot lock for synchronization because no clock has been defined";

                case 0x80040214:	//VFW_E_NO_SINK
                    return "Quality messages could not be sent because no quality sink has been defined";

                case 0x80040215:	//VFW_E_NO_INTERFACE
                    return "A required interface has not been implemented";

                case 0x80040216:	//VFW_E_NOT_FOUND
                    return "An object or name was not found";

                case 0x80040217:	//VFW_E_CANNOT_CONNECT
                    return "No combination of intermediate filters could be found to make the connection";

                case 0x80040218:	//VFW_E_CANNOT_RENDER
                    return "No combination of filters could be found to render the stream";

                case 0x80040219:	//VFW_E_CHANGING_FORMAT
                    return "Could not change formats dynamically";

                case 0x80040220:	//VFW_E_NO_COLOR_KEY_SET
                    return "No color key has been set";

                case 0x80040221:	//VFW_E_NO_DISPLAY_PALETTE
                    return "Display does not use a palette";

                case 0x80040222:	//VFW_E_TOO_MANY_COLORS
                    return "Too many colors for the current display settings";

                case 0x80040223:	//VFW_E_STATE_CHANGED
                    return "The state changed while waiting to process the sample";

                case 0x80040224:	//VFW_E_NOT_STOPPED
                    return "The operation could not be performed because the filter is not stopped";

                case 0x80040225:	//VFW_E_NOT_PAUSED
                    return "The operation could not be performed because the filter is not paused";

                case 0x80040226:	//VFW_E_NOT_RUNNING
                    return "The operation could not be performed because the filter is not running";

                case 0x80040227:	//VFW_E_WRONG_STATE
                    return "The operation could not be performed because the filter is in the wrong state";

                case 0x80040228:	//VFW_E_START_TIME_AFTER_END
                    return "The sample start time is after the sample end time";

                case 0x80040229:	//VFW_E_INVALID_RECT
                    return "The supplied rectangle is invalid";

                case 0x80040230:	//VFW_E_TYPE_NOT_ACCEPTED
                    return "This pin cannot use the supplied media type";

                case 0x80040231:	//VFW_E_CIRCULAR_GRAPH
                    return "The filter graph is circular";

                case 0x80040232:	//VFW_E_NOT_ALLOWED_TO_SAVE
                    return "Updates are not allowed in this state";

                case 0x80040233:	//VFW_E_TIME_ALREADY_PASSED
                    return "An attempt was made to queue a command for a time in the past";

                case 0x80040234:	//VFW_E_ALREADY_CANCELLED
                    return "The queued command has already been canceled";

                case 0x80040235:	//VFW_E_CORRUPT_GRAPH_FILE
                    return "Cannot render the file because it is corrupt";

                case 0x80040236:	//VFW_E_ADVISE_ALREADY_SET
                    return "An overlay advise link already exists";

                case 0x00040237:	//VFW_S_STATE_INTERMEDIATE
                    return "The state transition has not completed";

                case 0x80040239:	//VFW_E_NO_MODEX_AVAILABLE
                    return "This Advise cannot be canceled because it was not successfully set";

                case 0x80040240:	//VFW_E_NO_FULLSCREEN
                    return "The media type of this file is not recognized";

                case 0x80040241:	//VFW_E_CANNOT_LOAD_SOURCE_FILTER
                    return "The source filter for this file could not be loaded";

                case 0x00040242:	//VFW_S_PARTIAL_RENDER
                    return "Some of the streams in this movie are in an unsupported format";

                case 0x80040243:	//VFW_E_FILE_TOO_SHORT
                    return "A file appeared to be incomplete";

                case 0x80040244:	//VFW_E_INVALID_FILE_VERSION
                    return "The version number of the file is invalid";

                case 0x00040245:	//VFW_S_SOME_DATA_IGNORED
                    return "The file contained some property settings that were not used";

                case 0x00040246:	//VFW_S_CONNECTIONS_DEFERRED
                    return "Some connections have failed and have been deferred";

                case 0x00040103:	//VFW_E_INVALID_CLSID
                    return "A registry entry is corrupt";

                case 0x80040249:	//VFW_E_SAMPLE_TIME_NOT_SET
                    return "No time stamp has been set for this sample";

                case 0x00040250:	//VFW_S_RESOURCE_NOT_NEEDED
                    return "The resource specified is no longer needed";

                case 0x80040251:	//VFW_E_MEDIA_TIME_NOT_SET
                    return "No media time stamp has been set for this sample";

                case 0x80040252:	//VFW_E_NO_TIME_FORMAT_SET
                    return "No media time format has been selected";

                case 0x80040253:	//VFW_E_MONO_AUDIO_HW
                    return "Cannot change balance because audio device is mono only";

                case 0x00040260:	//VFW_S_MEDIA_TYPE_IGNORED
                    return "ActiveMovie cannot play MPEG movies on this processor";

                case 0x80040261:	//VFW_E_NO_TIME_FORMAT
                    return "Cannot get or set time related information on an object that is using a time format of TIME_FORMAT_NONE";

                case 0x80040262:	//VFW_E_READ_ONLY
                    return "The connection cannot be made because the stream is read only and the filter alters the data";

                case 0x00040263:	//VFW_S_RESERVED
                    return "This success code is reserved for internal purposes within ActiveMovie";

                case 0x80040264:	//VFW_E_BUFFER_UNDERFLOW
                    return "The buffer is not full enough";

                case 0x80040266:	//VFW_E_UNSUPPORTED_STREAM
                    return "Pins cannot connect due to not supporting the same transport";

                case 0x00040267:	//VFW_S_STREAM_OFF
                    return "The stream has been turned off";

                case 0x00040270:	//VFW_S_CANT_CUE
                    return "The stop time for the sample was not set";

                case 0x80040272:	//VFW_E_OUT_OF_VIDEO_MEMORY
                    return "The VideoPort connection negotiation process has failed";

                case 0x80040276:	//VFW_E_DDRAW_CAPS_NOT_SUITABLE
                    return "This User Operation is inhibited by DVD Content at this time";

                case 0x80040277:	//VFW_E_DVD_INVALIDDOMAIN
                    return "This Operation is not permitted in the current domain";

                case 0x00040280:	//VFW_E_DVD_NO_BUTTON
                    return "This object cannot be used anymore as its time has expired";

                case 0x80040281:	//VFW_E_DVD_WRONG_SPEED
                    return "The operation cannot be performed at the current playback speed";

                case 0x80040283:	//VFW_E_DVD_MENU_DOES_NOT_EXIST
                    return "The specified command was either cancelled or no longer exists";

                case 0x80040284:	//VFW_E_DVD_STATE_WRONG_VERSION
                    return "The data did not contain a recognized version";

                case 0x80040285:	//VFW_E_DVD_STATE_CORRUPT
                    return "The state data was corrupt";

                case 0x80040286:	//VFW_E_DVD_STATE_WRONG_DISC
                    return "The state data is from a different disc";

                case 0x80040287:	//VFW_E_DVD_INCOMPATIBLE_REGION
                    return "The region was not compatible with the current drive";

                case 0x80040288:	//VFW_E_DVD_NO_ATTRIBUTES
                    return "The requested DVD stream attribute does not exist";

                case 0x80040290:	//VFW_E_DVD_NO_GOUP_PGC
                    return "The current parental level was too low";

                case 0x80040291:	//VFW_E_DVD_INVALID_DISC
                    return "The specified path does not point to a valid DVD disc";

                case 0x80040292:	//VFW_E_DVD_NO_RESUME_INFORMATION
                    return "There is currently no resume information";

                case 0x80040295:	//VFW_E_PIN_ALREADY_BLOCKED_ON_THIS_THREAD
                    return "An operation failed due to a certification failure";

                case 0x80040296:	//VFW_E_VMR_NOT_IN_MIXER_MODE
                    return "The VMR could not find any ProcAmp hardware on the current display device";

                case 0x80040297:	//VFW_E_VMR_NO_AP_SUPPLIED
                    return "The application has not yet provided the VMR filter with a valid allocator-presenter object";

                case 0x80040298:	//VFW_E_VMR_NO_DEINTERLACE_HW
                    return "The VMR could not find any ProcAmp hardware on the current display device";

                case 0x80040299:	//VFW_E_VMR_NO_PROCAMP_HW
                    return "VMR9 does not work with VPE-based hardware decoders";

                case 0x8004029A:	//VFW_E_DVD_VMR9_INCOMPATIBLEDEC
                    return "VMR9 does not work with VPE-based hardware decoders";

                case 0x8004029B:	//VFW_E_NO_COPP_HW
                    return "The current display device does not support Content Output Protection Protocol (COPP) H/W";

                default:
                    return string.Empty;
            }
        }
    }
}