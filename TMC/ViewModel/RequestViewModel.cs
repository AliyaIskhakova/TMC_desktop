using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMC.Model;
using TMC.View;

namespace TMC.ViewModel
{
    public class RequestViewModel
    {
        ServiceCenterTMCEntities context = new ServiceCenterTMCEntities();
        RelayCommand? addCommand;
        RelayCommand? relayCommand;
        RelayCommand? editCommand;
        
        RelayCommand? selectCommand;
        RelayCommand? searchCommand;
        RelayCommand? deleteCommand;
        public List<Requests> Requests { get; set; }
        // команда добавления
        public RequestViewModel()
        {
            Requests = context.Requests.Local.ToList();
        }
        public RelayCommand AddCommand
        {
            get
            {
                return addCommand ??
                  (addCommand = new RelayCommand((o) =>
                  {
                      RequestWindow userWindow = new RequestWindow();
                      if (userWindow.ShowDialog() == true)
                      {
                          //User user = userWindow.User;
                          //db.Users.Add(user);
                          //db.SaveChanges();
                      }
                  }));
            }
        }
        // команда редактирования
        public RelayCommand EditCommand
        {
            get
            {
                return editCommand ??
                  (editCommand = new RelayCommand((selectedItem) =>
                  {
                      // получаем выделенный объект
                      Requests? request = selectedItem as Requests;
                      if (request == null) return;

                      Requests vm = new Requests
                      {
                          //Id = request.Id,
                          //Name = request.Name,
                          //Age = request.Age
                      };
                      RequestWindow requestWindow = new RequestWindow();


                      if (requestWindow.ShowDialog() == true)
                      {
                          //request.Name = userWindow.User.Name;
                          //user.Age = userWindow.User.Age;
                          //db.Entry(user).State = EntityState.Modified;
                          //db.SaveChanges();
                      }
                  }));
            }
        }
        // команда удаления
        //public RelayCommand DeleteCommand
        //{
        //    get
        //    {
        //        return deleteCommand ??
        //          (deleteCommand = new RelayCommand((selectedItem) =>
        //          {
        //              // получаем выделенный объект
        //              User? user = selectedItem as User;
        //              if (user == null) return;
        //              db.Users.Remove(user);
        //              db.SaveChanges();
        //          }));
        //    }
        //}
    }
}
