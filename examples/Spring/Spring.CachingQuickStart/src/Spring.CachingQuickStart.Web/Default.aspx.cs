using System;
using System.Collections.Generic;

using Spring.Context;
using Spring.Context.Support;
using Spring.Caching;

using Spring.CachingQuickStart.Services;

namespace Spring.CachingQuickStart.Web
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        // GetById
        protected void GetByIdButton_Click(object sender, EventArgs e)
        {
            IApplicationContext ctx = ContextRegistry.GetContext();
            IMovieService movieService = ctx.GetObject("MovieService") as IMovieService;

            Movie movie = movieService.GetById(Int32.Parse(this.GetByIdTextBox.Text));

            this.GetByIdLabel.Text = movie.Title;

            UpdateCacheContent();
        }

        // FindAll
        protected void FindAllButton_Click(object sender, EventArgs e)
        {
            IApplicationContext ctx = ContextRegistry.GetContext();
            IMovieService movieService = ctx.GetObject("MovieService") as IMovieService;

            IEnumerable<Movie> movies = movieService.FindAll();

            this.FindAllRepeater.DataSource = movies;
            this.FindAllRepeater.DataBind();

            UpdateCacheContent();
        }

        // Save
        protected void SaveButton_Click(object sender, EventArgs e)
        {
            IApplicationContext ctx = ContextRegistry.GetContext();
            IMovieService movieService = ctx.GetObject("MovieService") as IMovieService;

            Movie movie = new Movie(
                Int32.Parse(this.SaveIdTextBox.Text),
                this.SaveTitleTextBox.Text);

            movieService.Save(movie);

            UpdateCacheContent();
        }

        // Delete
        protected void DeleteButton_Click(object sender, EventArgs e)
        {
            IApplicationContext ctx = ContextRegistry.GetContext();
            IMovieService movieService = ctx.GetObject("MovieService") as IMovieService;

            Movie movie = new Movie(
                Int32.Parse(this.DeleteIdTextBox.Text),
                "NotUsed");

            movieService.Delete(movie);

            UpdateCacheContent();
        }

        
        // Using Caching API programmatically

        protected void ClearButton_Click(object sender, EventArgs e)
        {
            ClearCacheContent();
            UpdateCacheContent();
        }

        protected void RefreshButton_Click(object sender, EventArgs e)
        {
            UpdateCacheContent();
        }

        private void ClearCacheContent()
        {
            IApplicationContext ctx = ContextRegistry.GetContext();
            ICache cache = ctx.GetObject("DefaultCache") as ICache;

            cache.Clear();
        }

        private void UpdateCacheContent()
        {
            IList<CacheEntry> cacheEntries = new List<CacheEntry>();

            IApplicationContext ctx = ContextRegistry.GetContext();
            ICache cache = ctx.GetObject("DefaultCache") as ICache;

            foreach (object key in cache.Keys)
            {
                cacheEntries.Add(new CacheEntry() { Key = key, Value = cache.Get(key) });
            }

            this.CacheRepeater.DataSource = cacheEntries;
            this.CacheRepeater.DataBind();
        }

        public class CacheEntry
        {
            public object Key { get; set; }
            public object Value { get; set; }
        }
    }
}
