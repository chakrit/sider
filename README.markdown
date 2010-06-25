Redis bindings for C#

Copyright (c) 2010 Chakrit Wichian.
All rights reserved.

**WARNING:** The source code is _not_ for the feint of heart.
        
To keep the code DRY and still maintain both ReadXXX and BeginReadXXX methods
I've decided to throw in some functional kung-fu there by separating the actual
stream-reading function and the offset calculation logic from each other,
thus resulting in some hardcore use of callbacks/continuation-passing-style.
