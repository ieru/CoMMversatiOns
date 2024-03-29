﻿/**
 *  CoMMversatiOns Project: API Tool for increase client functionalities of
 *  realXtend technologies.
 *  Copyright (C) 2010 Information Engineering Research Unit
 *  
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *  
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoMMversatiOns.Events
{
    /// <summary>
    /// Data of a chat event.
    /// </summary>
    public class ChatActionEventArgs : EventArgs
    {
        /// <summary>
        /// User that sent the chat message.
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// Message sent by the user.
        /// </summary>
        public string Message { get; set; }
    }
}
