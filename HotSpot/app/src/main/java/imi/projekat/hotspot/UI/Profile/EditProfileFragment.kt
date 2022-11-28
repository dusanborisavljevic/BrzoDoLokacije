package imi.projekat.hotspot.UI.Profile

import android.content.ContentResolver
import android.content.Context
import android.content.Intent
import android.graphics.Bitmap
import android.graphics.BitmapFactory
import android.graphics.ImageDecoder
import android.net.Uri
import android.os.Build
import android.os.Bundle
import android.provider.MediaStore
import android.provider.OpenableColumns
import android.util.Base64
import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import android.widget.ImageView
import android.widget.TextView
import android.widget.Toast
import androidx.activity.result.ActivityResultLauncher
import androidx.activity.result.ActivityResultRegistry
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AppCompatActivity
import androidx.fragment.app.Fragment
import androidx.fragment.app.activityViewModels
import androidx.lifecycle.DefaultLifecycleObserver
import androidx.lifecycle.LifecycleOwner
import androidx.lifecycle.lifecycleScope
import androidx.navigation.fragment.findNavController
import com.auth0.android.jwt.JWT
import com.google.android.material.textfield.TextInputLayout
import imi.projekat.hotspot.Ostalo.*
import imi.projekat.hotspot.R
import imi.projekat.hotspot.ViewModeli.MainActivityViewModel
import imi.projekat.hotspot.databinding.FragmentEditProfileBinding
import kotlinx.android.synthetic.main.fragment_edit_profile.*
import kotlinx.coroutines.flow.collectLatest
import kotlinx.coroutines.launch
import okhttp3.MediaType.Companion.toMediaTypeOrNull
import okhttp3.MultipartBody
import okhttp3.RequestBody.Companion.asRequestBody
import java.io.File
import java.io.FileInputStream
import java.io.FileOutputStream

// TODO: Rename parameter arguments, choose names that match
// the fragment initialization parameters, e.g. ARG_ITEM_NUMBER
private const val ARG_PARAM1 = "param1"
private const val ARG_PARAM2 = "param2"

/**
 * A simple [Fragment] subclass.
 * Use the [EditProfileFragment.newInstance] factory method to
 * create an instance of this fragment.
 */

class MyLifecycleObserver(private val registry : ActivityResultRegistry)
    : DefaultLifecycleObserver {
    lateinit var getContent : ActivityResultLauncher<String>

    override fun onCreate(owner: LifecycleOwner) {
        getContent = registry.register("key", owner, ActivityResultContracts.GetContent()) { uri ->
            // Handle the returned Uri
        }
    }

    fun selectImage() {
        getContent.launch("image/*")
    }
}


class EditProfileFragment : Fragment() {
    private lateinit var binding:FragmentEditProfileBinding
    private lateinit var imageView: ImageView
    private lateinit var file: File
    private lateinit var uri : Uri
    private lateinit var camIntent: Intent
    private lateinit var galIntent:Intent
    private lateinit var cropIntent:Intent
    private lateinit var btnImg: Button
    private lateinit var jwt: JWT
    private lateinit var profileImage:ImageView
    private lateinit var username: TextView
    private lateinit var email: TextView
    private var selectedImageUri: Uri?=null
    private lateinit var observer : MyLifecycleObserver
    private var photoBitmap:Bitmap?=null
    private var bitmapaSlike:Bitmap?=null
    private val contract= registerForActivityResult(ActivityResultContracts.StartActivityForResult()) {
        // Handle the returned Uri
        selectedImageUri=it.data!!.data
        //val bitmap = MediaStore.Images.Media.getBitmap(this.getContentResolver(), imageUri)
        imageView.setImageURI(selectedImageUri)
        val slika= it.data!!.data
        val resolver = requireActivity().contentResolver
        if(resolver!=null){
            if (Build.VERSION.SDK_INT >= 28) {
                val source= ImageDecoder.createSource(resolver, slika!!)
                bitmapaSlike= ImageDecoder.decodeBitmap(source)

            } else {
                bitmapaSlike=MediaStore.Images.Media.getBitmap(resolver,slika)

            }
            //slikaView.setImageBitmap(bitmap)
        }
    }

//    val startForResult = registerForActivityResult(ActivityResultContracts.StartActivityForResult()) { result: ActivityResult ->
//        if (result.resultCode == Activity.RESULT_OK) {
//            val intent = result.data
//            // Handle the Intent
//            selectedImageUri=intent?.data!!
//            profileImage.setImageURI(selectedImageUri)
//        }
//    }

    private val viewModel: MainActivityViewModel by activityViewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        observer = MyLifecycleObserver(requireActivity().activityResultRegistry)
        lifecycle.addObserver(observer)
    }

    private fun setup(){
        val token= MenadzerSesije.getToken(requireContext())


    }

    override fun onCreateView(
        inflater: LayoutInflater, container: ViewGroup?,
        savedInstanceState: Bundle?
    ): View? {
        binding=FragmentEditProfileBinding.inflate(inflater)
        val view=inflater.inflate(R.layout.fragment_edit_profile,container,false)
        imageView=view.findViewById(binding.profileImage1.id)

        val token= MenadzerSesije.getToken(requireContext())
        if(token != null)
        {
            jwt= JWT(token)
            val usernameToken=jwt.getClaim("username").asString()
            val emailToken=jwt.getClaim("email").asString()
            username=view.findViewById(binding.EditUsername.id)
            email=view.findViewById(binding.EditEmail.id)
            username.text=usernameToken
            email.text=emailToken
            var photoPath = jwt.getClaim("photo").asString()!!
            if(!photoPath.isNullOrEmpty()){
                val pom2=photoPath.split("\\")
                viewModel.dajSliku(imageView,"ProfileImages/"+pom2[2],this.requireContext())
            }


        }


        return view

    }



    override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
        super.onViewCreated(view, savedInstanceState)
        binding= FragmentEditProfileBinding.bind(view)

        binding.EditProfileImage.setOnClickListener{
            galerija()
        }

        viewLifecycleOwner.lifecycleScope.launch{
            viewModel.liveEditProfileResponse.collectLatest{
                if(it is BaseResponse.Error){
                    val id = UpravljanjeResursima.getResourceString(it.poruka.toString(),requireContext())
                    Toast.makeText(requireContext(), id, Toast.LENGTH_SHORT).show()
                }
                if(it is BaseResponse.Success){
                    Log.d("SES","SESESSE")
                    val id = UpravljanjeResursima.getResourceString(it.data?.message.toString(),requireContext())
                    Toast.makeText(requireContext(), id, Toast.LENGTH_SHORT).show()
                    if(!it.data?.token.isNullOrEmpty())
                    {
                        MenadzerSesije.saveAuthToken(requireContext(),it.data?.token.toString())
                    }
                    findNavController().navigate(R.id.action_editProfileFragment_to_myProfileFragment)
                }
            }
        }

//        viewModel.liveEditProfileResponse.observe(viewLifecycleOwner){
//            if(it is BaseResponse.Error){
//                Toast.makeText(context, it.poruka, Toast.LENGTH_SHORT).show()
//            }
//            if(it is BaseResponse.Success){
//                val id = UpravljanjeResursima.getResourceString(it.data?.message.toString(),requireContext())
//                Toast.makeText(requireContext(), id, Toast.LENGTH_SHORT).show()
//                if(!it.data?.token.isNullOrEmpty())
//                {
//                    MenadzerSesije.saveAuthToken(requireContext(),it.data?.token.toString())
//                }
//
//                findNavController().navigate(R.id.action_editProfileFragment_to_myProfileFragment)
//
//            }
//
//        }

        binding.changeDugme.setOnClickListener {
            upload()
        }


    }

    override fun onDestroy() {
        super.onDestroy()
        Log.d("unistavanje","brzi")


    }

    //SLIKAAAAAAA

    private fun galerija() {
        val intent = Intent()
        intent.type = "image/*"
        intent.putExtra(Intent.EXTRA_ALLOW_MULTIPLE, true)
        intent.action=Intent.ACTION_GET_CONTENT
        try {
            contract.launch(intent)
        }
        catch (e:Exception) {
            Log.d("GRESKA",e.toString())
        }
    }



    private fun upload(){
        val pomUsername=binding.EditUsername.text.toString().trim()
        val pomEmail=binding.EditEmail.text.toString().trim()
        val pomOldPassword=binding.OldPassword.text.toString().trim()
        val pomNewPasswrod=binding.NewPassword.text.toString().trim()
        Log.d("zicla",pomUsername)
        if(pomUsername.isBlank())
        {
            binding.EditUsername.setError(getString(R.string.InsertYourUsername))
            return
        }
        if(pomUsername.length<4)
        {
            binding.EditUsername.setError(getString(R.string.UserNameShortLength))
            return
        }

        if(pomUsername.length>20)
        {
            binding.EditUsername.setError(getString(R.string.UserNameLongLength))
            return
        }

        if(pomEmail.isBlank())
        {
            binding.EditEmail.setError(getString(R.string.InsertYourEmail))
            return
        }

        val regexforemail="^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$".toRegex()
        if(!regexforemail.matches(pomEmail))
        {
            binding.EditEmail.setError(getString(R.string.ErrorEmailForm))
            return
        }


        if(pomOldPassword.isBlank())
        {
            binding.passwordWrapper.endIconMode= TextInputLayout.END_ICON_NONE
            binding.OldPassword.setError(getString(R.string.OldPasswordEmpty))
            return
        }

        if(pomNewPasswrod.isNotBlank())
        {
            if(pomNewPasswrod.length<5)
            {
                binding.passwordWrapper1.endIconMode= TextInputLayout.END_ICON_NONE
                binding.NewPassword.setError(getString(R.string.ShortPasswordLength))
                return
            }

            if(pomNewPasswrod.length>20)
            {
                binding.passwordWrapper1.endIconMode= TextInputLayout.END_ICON_NONE
                binding.NewPassword.setError(getString(R.string.PasswordLongLegth))
                return
            }

            if(!pomNewPasswrod.equals(binding.ConfirmnewPassword1.text.toString()))
            {
                binding.passwordWrapper2.endIconMode= TextInputLayout.END_ICON_NONE
                binding.ConfirmnewPassword1.setError(getString(R.string.PasswordsAreNotTheSame))
                return
            }
        }
        val usernameToken1=jwt.getClaim("username").asString()
        val emailToken1=jwt.getClaim("email").asString()
        if(usernameToken1.equals(pomUsername) && emailToken1.equals(pomEmail) && selectedImageUri==null && pomNewPasswrod.isBlank()){
            Log.d("Ne valja","Unesite izmenu")
            return
        }

        val username=MultipartBody.Part.createFormData("Username",pomUsername)
        val email=MultipartBody.Part.createFormData("Email",pomEmail)
        val oldpassword=MultipartBody.Part.createFormData("OldPassword",pomOldPassword)
        val newpassword=MultipartBody.Part.createFormData("NewPassword",pomNewPasswrod)//MOGUC BAG

        if(selectedImageUri!=null)
        {
            val parcelFileDescriptor=getActivity()?.contentResolver?.openFileDescriptor(selectedImageUri!!,"r",null)?:return
            val inputStream=FileInputStream(parcelFileDescriptor.fileDescriptor)
            val file=File(getActivity()?.cacheDir,getActivity()?.contentResolver?.getFileName(selectedImageUri!!))
            val outputStream=FileOutputStream(file)
            inputStream.copyTo(outputStream)
            val requestBody=file.asRequestBody("slika".toMediaTypeOrNull())
            val part=MultipartBody.Part.createFormData("slika",file.name,requestBody)
            viewModel.ChangeProfilePhoto(part,username,email,oldpassword,newpassword)
        }
        else
        {
            val part=MultipartBody.Part.createFormData("slika","")
            viewModel.ChangeProfilePhoto(part,username,email,oldpassword,newpassword)
        }



    }



    fun ContentResolver.getFileName(fileUri: Uri): String {
        var name = ""
        val returnCursor = this.query(fileUri, null, null, null, null)
        if (returnCursor != null) {
            val nameIndex = returnCursor.getColumnIndex(OpenableColumns.DISPLAY_NAME)
            returnCursor.moveToFirst()
            name = returnCursor.getString(nameIndex)
            returnCursor.close()
        }
        return name
    }

    companion object {
        const val REQUEST_CODE_PICK_IMAGE = 101
    }










    }
