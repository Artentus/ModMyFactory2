//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

namespace ModMyFactory.IO
{
    interface ISymlinkInfo
    {
        string Name { get; }

        string FullName { get; }

        string DestinationPath { get; set; }

        bool Exists { get; }

        void Create(string desination);

        void Delete();
    }
}
