﻿using HotSpotAPI.Data;
using HotSpotAPI.Modeli;
using HotSpotAPI.ModeliZaZahteve;

namespace HotSpotAPI.Servisi
{
    public interface IPostService
    {
        public bool addNewPost(int id, addPost newPost);
        public List<getPosts> getAllPosts();
        public List<getPosts> getAllPosts(int id);
        public getPosts getPost(int id, int postID);
        public bool deletePost(int id, int postID);
        public List<getPosts> getAllPostsByLocaton(string location);
        public bool addComment(int id, comment comm);
        public List<comments> GetComments(int postid);
        public bool DeleteComment(int commid, int postid, int userid);
        public bool EditComment(int commid, int postId, string newtext, int id);
        public bool addLike(int id, int postid);
        public bool dislike(int id, int postid);
        public List<likes> getLikes(int id);
        public List<comments> GetReplies(int postId, int commid);
        public bool addCommLike(int id, int postid, int commid);
        public bool dislikeComm(int id, int postid, int commid);
    }
    public class PostService : IPostService
    {
        private readonly IHttpContextAccessor httpContext;
        private MySqlDbContext context;
        private readonly ImailService mailService;
        private readonly IMySQLServis mysqlServis;
        private readonly IStorageService storageService;

        public PostService(IHttpContextAccessor httpContext, MySqlDbContext context, ImailService mailService, IMySQLServis mysqlServis, IStorageService storageService)
        {
            this.httpContext = httpContext;
            this.context = context;
            this.mailService = mailService;
            this.mysqlServis = mysqlServis;
            this.storageService = storageService;
        }
        public bool addNewPost(int id, addPost newPost)
        {
            var user = context.Korisnici.Find(id);
            if (user == null)
                return false;

            Post p = new Post();
            p.UserID = id;
            p.Description = newPost.description;
            p.Location = newPost.location;
            p.DateTime = DateTime.Now;
            p.NumOfPhotos = newPost.photos.Count;
            p.shortDescription = newPost.shortDescription;
            context.Postovi.Add(p);
            context.SaveChanges();

            string basepath = storageService.CreatePost();
            if (!Directory.Exists(basepath))
                Directory.CreateDirectory(basepath);

            int brojac = 1;
            foreach (IFormFile slika in newPost.photos)
            {
                string path = Path.Combine(basepath, "user" + id + "post" + p.ID + "photo" + brojac + ".jpg");
                using (FileStream stream = System.IO.File.Create(path))
                {
                    slika.CopyTo(stream);
                    stream.Flush();
                }
                brojac += 1;
            }
            return true;
        }
        public List<getPosts> getAllPosts(int id)
        {
            List<Post> posts = context.Postovi.Where(x => x.UserID == id).ToList();
            var kor = context.Korisnici.Find(id);
            List<getPosts> postsList = new List<getPosts>();

            foreach (Post post in posts)
            {
                getPosts p = new getPosts();
                p.username = kor.Username;
                p.ownerID = kor.ID;
                string basepath1 = storageService.CreatePhoto();
                if (kor.ProfileImage == "" || kor.ProfileImage == null)
                    p.profilephoto = "";
                else
                {
                    string path = Path.Combine(basepath1, "user" + id+".jpg");
                    Byte[] b = System.IO.File.ReadAllBytes(path);
                    p.profilephoto = Convert.ToBase64String(b, 0, b.Length);
                }
                p.description = post.Description;
                p.location = post.Location;
                p.DateTime = post.DateTime;
                p.photos = new List<string>();
                p.brojslika = post.NumOfPhotos;
                p.brojlajkova = post.NumOfLikes;
                p.shortDescription = post.shortDescription;
                p.postID = post.ID;
                string basepath = storageService.CreatePost();
                basepath = Path.Combine(basepath, "user" + id + "post" + post.ID);
                for (int i = 1; i <= post.NumOfPhotos; i++)
                {
                    string path = Path.Combine(basepath + "photo" + i + ".jpg");
                    Byte[] b = System.IO.File.ReadAllBytes(path);
                    string slika = Convert.ToBase64String(b, 0, b.Length);
                    p.photos.Add(slika);
                }
                postsList.Add(p);
            }

            return postsList;
        }
        public List<getPosts> getAllPosts()
        {
            List<Post> posts = context.Postovi.Where(x => x.UserID>0).ToList();
            List<getPosts> postsList = new List<getPosts>();

            foreach (Post post in posts)
            {
                var kor = context.Korisnici.Find(post.UserID);
                if (kor == null)
                    return null;
                getPosts p = new getPosts();
                p.username = kor.Username;
                p.ownerID = kor.ID;
                string basepath1 = storageService.CreatePhoto();
                if (kor.ProfileImage == "" || kor.ProfileImage == null)
                    p.profilephoto = "";
                else
                {
                    string path = Path.Combine(basepath1, "user" + kor.ID + ".jpg");
                    Byte[] b = System.IO.File.ReadAllBytes(path);
                    p.profilephoto = Convert.ToBase64String(b, 0, b.Length);
                }
                p.description = post.Description;
                p.location = post.Location;
                p.DateTime = post.DateTime;
                p.photos = new List<string>();
                p.brojslika = post.NumOfPhotos;
                p.shortDescription = post.shortDescription;
                p.brojlajkova = post.NumOfLikes;
                p.postID = post.ID;
                string basepath = storageService.CreatePost();
                basepath = Path.Combine(basepath, "user" + kor.ID + "post" + post.ID);
                for (int i = 1; i <= post.NumOfPhotos; i++)
                {
                    string path = Path.Combine(basepath + "photo" + i + ".jpg");
                    Byte[] b = System.IO.File.ReadAllBytes(path);
                    string slika = Convert.ToBase64String(b, 0, b.Length);
                    p.photos.Add(slika);
                }
                postsList.Add(p);
            }

            return postsList;
        }
        public List<getPosts> getAllPostsByLocaton(string location)
        {
            List<Post> posts = context.Postovi.Where(x => x.Location.Equals(location)).ToList();
            List<getPosts> postsList = new List<getPosts>();

            foreach (Post post in posts)
            {
                getPosts p = new getPosts();
                p.description = post.Description;
                p.location = post.Location;
                p.DateTime = post.DateTime;
                p.photos = new List<string>();
                p.brojslika = post.NumOfPhotos;
                p.shortDescription = post.shortDescription;
                p.postID = post.ID;
                string basepath = storageService.CreatePost();
                basepath = Path.Combine(basepath, "user" + post.UserID + "post" + post.ID);
                for (int i = 1; i <= post.NumOfPhotos; i++)
                {
                    string path = Path.Combine(basepath + "photo" + i + ".jpg");
                    Byte[] b = System.IO.File.ReadAllBytes(path);
                    string slika = Convert.ToBase64String(b, 0, b.Length);
                    p.photos.Add(slika);
                }
                postsList.Add(p);
            }

            return postsList;
        }
        public getPosts getPost(int id, int postID)
        {
            Post post = context.Postovi.FirstOrDefault(x => x.UserID == id && x.ID == postID);
            if (post == null)
                return null;
            getPosts p = new getPosts();
            p.description = post.Description;
            p.location = post.Location;
            p.DateTime = post.DateTime;
            p.photos = new List<string>();
            p.brojslika = post.NumOfPhotos;
            p.shortDescription = post.shortDescription;
            p.postID = post.ID;
            string basepath = storageService.CreatePost();
            basepath = Path.Combine(basepath, "user" + id + "post" + post.ID);
            for (int i = 1; i <= post.NumOfPhotos; i++)
            {
                string path = Path.Combine(basepath + "photo" + i + ".jpg");
                Byte[] b = System.IO.File.ReadAllBytes(path);
                string slika = Convert.ToBase64String(b, 0, b.Length);
                p.photos.Add(slika);
            }
            return p;
        }
        public bool deletePost(int id, int postID)
        {
            Post post = context.Postovi.FirstOrDefault(x => x.UserID == id && x.ID == postID);
            if (post == null)
                return false;

            bool res = storageService.deletePost(id, postID, post.NumOfPhotos);
            if (!res)
                return false;

            List<Komentari> coms = context.Komentari.Where(x => x.PostID == postID).ToList();
            foreach (Komentari k in coms)
            {
                context.Komentari.Remove(k);
                context.SaveChanges();
            }

            List<Like> lajkovi = context.Likes.Where(x => x.PostID == postID).ToList();
            foreach (Like like in lajkovi)
            {
                context.Remove(like);
                context.SaveChanges();
            }
            context.Remove(post);
            context.SaveChanges();
            return true;
        }
        public bool addComment(int id, comment comm)
        {
            var post = context.Postovi.FirstOrDefault(x => x.ID == comm.postid);
            if (post == null)
                return false;

            Komentari kom = new Komentari();
            kom.PostID = comm.postid;
            kom.DateTime = DateTime.Now;
            kom.Text = comm.text;
            kom.UserID = id;
            kom.ParentID = comm.parentid;
            context.Komentari.Add(kom);
            context.SaveChanges();

            return true;
        }

        public List<comments> GetComments(int postid)
        {
            var komentari = context.Komentari.Where(x => x.PostID == postid && x.ParentID == 0).ToList();
            if (komentari == null)
                return null;

            List<comments> koms = new List<comments>();
            foreach (Komentari c in komentari)
            {
                comments kom = new comments();
                kom.OwnerID = c.UserID;
                var user = context.Korisnici.FirstOrDefault(x => x.ID == c.UserID);
                if (user == null)
                    return null;
                string photopath = user.ProfileImage;
                if (photopath == "" || photopath == null)
                    kom.userPhoto = "";
                else
                {
                    byte[] b = System.IO.File.ReadAllBytes(photopath);
                    string slika = Convert.ToBase64String(b, 0, b.Length);
                    kom.userPhoto = slika;
                }
                kom.username = user.Username;
                kom.text = c.Text;
                kom.time = c.DateTime;
                kom.NumOfLikes = c.NumOFLikes;
                koms.Add(kom);
            }
            return koms;
        }
        public List<comments> GetReplies(int postId, int commid)
        {
            var komentari = context.Komentari.Where(x => x.PostID == postId && x.ParentID == commid).ToList();
            if (komentari == null)
                return null;

            List<comments> koms = new List<comments>();
            foreach (Komentari c in komentari)
            {
                comments kom = new comments();
                kom.OwnerID = c.UserID;
                var user = context.Korisnici.FirstOrDefault(x => x.ID == c.UserID);
                if (user == null)
                    return null;
                string photopath = user.ProfileImage;
                if (photopath == "" || photopath == null)
                    kom.userPhoto = "";
                else
                {
                    byte[] b = System.IO.File.ReadAllBytes(photopath);
                    string slika = Convert.ToBase64String(b, 0, b.Length);
                    kom.userPhoto = slika;
                }

                kom.username = user.Username;
                kom.text = c.Text;
                kom.time = c.DateTime;

                koms.Add(kom);
            }
            return koms;
        }
        public bool DeleteComment(int commid, int postid, int userid)
        {
            var com = context.Komentari.FirstOrDefault(x => x.ID == commid && x.PostID == postid && x.UserID == userid);
            if (com == null)
                return false;

            context.Komentari.Remove(com);
            context.SaveChanges();
            return true;
        }
        public bool EditComment(int commid, int postId, string newtext, int id)
        {
            var com = context.Komentari.FirstOrDefault(x => x.ID == commid && x.PostID == postId && x.UserID == id);
            if (com == null)
                return false;

            com.Text = newtext;
            com.DateTime = DateTime.Now;
            context.SaveChanges();

            return true;
        }

        public bool addLike(int id, int postid)
        {
            var post = context.Postovi.FirstOrDefault(x => x.ID == postid);
            if (post == null)
                return false;
            post.NumOfLikes++;

            Like l = new Like();
            l.PostID = postid;
            l.UserID = id;
            context.Likes.Add(l);
            context.SaveChanges();

            return true;
        }
        public bool dislike(int id, int postid)
        {
            var like = context.Likes.FirstOrDefault(x => x.UserID == id && x.PostID == postid);
            if (like == null)
                return false;

            context.Likes.Remove(like);
            context.SaveChanges();

            var post = context.Postovi.FirstOrDefault(x => x.ID == postid);
            if (post == null)
                return false;
            post.NumOfLikes--;
            context.SaveChanges();
            return true;
        }
        public List<likes> getLikes(int id)
        {
            List<likes> lista = new List<likes>();
            var likes = context.Likes.Where(x => x.UserID == id).ToList();
            if (likes == null)
                return null;

            foreach (Like like in likes)
            {
                likes l = new likes();
                l.postid = like.PostID;
                lista.Add(l);
            }

            return lista;
        }
        public bool addCommLike(int id, int postid, int commid)
        {
            var komentar = context.Komentari.FirstOrDefault(x => x.UserID != id && x.ID == commid && x.PostID == postid);
            if (komentar == null)
                return false;
            komentar.NumOFLikes++;

            LikeKomentara l = new LikeKomentara();
            l.PostID = postid;
            l.UserID = id;
            l.CommentID = commid;
            context.LikeKomentara.Add(l);
            context.SaveChanges();

            return true;
        }

        public bool dislikeComm(int id, int postid, int commid)
        {
            var like = context.LikeKomentara.FirstOrDefault(x => x.UserID == id && x.PostID == postid && x.CommentID == commid);
            if (like == null)
                return false;

            context.LikeKomentara.Remove(like);
            context.SaveChanges();

            var kom = context.Komentari.FirstOrDefault(x => x.ID == commid && x.ID == postid);
            if (kom == null)
                return false;
            kom.NumOFLikes--;
            context.SaveChanges();
            return true;
        }
    }
}