# LETS for Orchard

A LETS accounting system based on Orchard https://github.com/OrchardCMS/Orchard

## Setup
1. Download or clone & build
2. Create a database & ensure access
3. Navigate to the website at /src/Orchard.web, select the Default recipe & cook
4. Go to the Dashboard, and enable the LETS module
5. Configure settings as per alerts
6. Create 3 notice types - Offer, Request, Announcement
7. Create some localities
8. Enable the LETS Bootstrap theme
9. Go to taxonomies and add some terms to the Category taxonomy.  You can import some suggested categories from the file in src\Orchard.Web\Modules\LETS\Data\categories.txt. After importing the suggested categories, edit each root category so it's not selectable.
10. Go to Navigation & add links to Offers, Requests & Announcements (if you wish) as follows: Open /Admin/Contents/List/NoticeType in a new tab and hover over the items for Offer, Request, Announcement to discover their ids. Now supposing the Offer notice type has and id of 13, add a Custom Link to the navigation with the Menu text of 'Offers' and a Url /lets/noticetype/list/13.  Do the same for the other 2 notice types.
11. Add a custom link to the main menu for "Become a member" at the url /users/account/registermember
12. Add more links to the main menu as required for your blog, about, contact pages etc.
13. Add a second navigation section for authenticated users as follows:  Go to Widgets & add an Auth Navigation widget to the AuthNavigation zone. Select the authenticated layer, give it a title but uncheck "render the title". Save.  You should now see some extra navigation on the home page.

