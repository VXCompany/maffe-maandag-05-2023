﻿
//vreate an endpoint to return bookmarks with a url, profile id and read boolean
//create the bookmark class
public class Bookmark
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    public string ProfileId { get; set; }
    public bool Read { get; set; }
}
