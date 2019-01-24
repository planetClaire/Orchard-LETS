# LETS for Orchard

A LETS accounting system based on Orchard https://github.com/OrchardCMS/Orchard

## Setup
### Download & build
1. Download or clone & build
2. Create a database & ensure access
3. Navigate to the website at /src/Orchard.web, select the Default recipe & cook
### Configure the LETS module
1. Go to the Dashboard, and enable the LETS module
2. Configure settings as per alerts
3. Enable the LETS Bootstrap theme
4. Optionally set the background color & logo in the theme settings

### Configure the LETS system itself
1. Create 3 notice types - Offer, Request, Announcement
2. Create some localities
3. Go to taxonomies and add some terms to the Category taxonomy.  You can import some suggested categories from the file in src\Orchard.Web\Modules\LETS\Data\categories.txt. After importing the suggested categories, edit each root category so it's not selectable. 
4. Add and enable the website culture in Settings
4. Fill out the details of the admin account - edit the admin account from Users, or from the LETS link in the dashboard (underneath Content).  This account will need:
  a. an Email address (for members to contact) - also change the User Name to this email address.
  b. a Join date
  c. a Type of Admin
  d. First, last name (eg: Admin LETS), telephone, street address (can be na)
  e. A Locality
  f. Check "Administrator" if it's not already checked

### Configure user settings
In Settings, Users, set the following:
1. Check "Users can create new accounts on the site"
2. Check "Display a link to enable users to reset their password"
3. Check "Users must verify their email address"
4. Add a website public name
5. Add a contact us email address
6. Check "Users must be approved before they can log in"
7. Check "Send a notification when a user needs moderation"
8. Add the admin email address to the Moderators box

### Add some navigation to the website
1. Go to Navigation & add links to Offers, Requests & Announcements (if you wish) as follows: Open /Admin/Contents/List/NoticeType in a new tab and hover over the items for Offer, Request, Announcement to discover their ids. Now supposing the Offer notice type has and id of 2. add a Custom Link to the navigation with the Menu text of 'Offers' and a Url /lets/noticetype/list/13.  Do the same for the other 2 notice types.
3. Add a custom link to the main menu for "Become a member" at the url /users/account/registermember
4. Add more links to the main menu as required for your blog, about, contact pages etc.
5. Add a second navigation section for authenticated users as follows:  Go to Widgets & add an Auth Navigation widget to the AuthNavigation zone. Select the authenticated layer, give it a title but uncheck "render the title". Save.  You should now see some extra navigation on the home page.

### Help new members get signed up & approved
When new members complete the "Become a member" form, they'll be sent a verification email & directed to a page headed "Next steps". The content of this page needs to be expanded.  
1. Add a new widget layer called "challenge email sent" with a layer rule of url("~/lets/account/challengeemailsent") 
2. Then add a HTML widget to the Content zone of the widget layer you just created, and populate it. Some example HTML is in src\Orchard.Web\Modules\LETS\Data\next-steps.html
3. You can also add some content to the page that users land on when they click their verification links.  To do this, add a widget layer called "challenge email success" with a rule of url("~/users/account/challengeemailsuccess"), and populate it with a HTML widget - the example content is in challenge-email-success.html
