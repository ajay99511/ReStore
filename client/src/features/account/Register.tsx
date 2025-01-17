import Box from '@mui/material/Box';
import FormControl from '@mui/material/FormControl';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import { Avatar, Container, Paper } from '@mui/material';
import { LockOutlined } from '@mui/icons-material';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { LoadingButton } from '@mui/lab';
import agent from '../../app/api/agent';
import { toast } from 'react-toastify';

export default function Register() {
    const navigate = useNavigate();
    const {register,handleSubmit,setError,formState:{isSubmitting,isValid,errors}} = useForm(
      {
        mode:'onTouched'
      }
    );

    function ApiErrorHandler(errors:any)
    {
        if(errors)
        {
            errors.forEach((error:string) => {
               if(error.includes('Password'))
               {
                setError('password',{message:error})
               }
               else if(error.includes('email'))
                {
                 setError('email',{message:error})
                }
                else{
                    setError('username',{message:error})
                }
            });
        }
    }


  return (
    <>
    <Container component={Paper} maxWidth="sm"  
    sx={{display:'flex', flexDirection:'column',alignItems:'center',padding:4}}>
        <Avatar
        sx={{m:1,bgcolor:'secondary.main'}}
        >
            <LockOutlined/>
        </Avatar>
            <Typography
              component="h1"
              variant="h4"
            >
              Register
            </Typography>
            <Box
              component="form"
              onSubmit={handleSubmit(
                data=>agent.Account.register(data).then(()=>
                {
                    toast.success("Registered Successfully");
                    navigate('/login');
                })
                .catch(error=>ApiErrorHandler(error)))}
              noValidate
              sx={{
                display: 'flex',
                flexDirection: 'column',
                width: '80%',
                gap: 2,
                mt:1
              }}
            >
              <FormControl>
                <TextField
                  placeholder="username"
                  label="username"
                  autoFocus
                  fullWidth
                  variant="outlined"
                  sx={{ ariaLabel: 'username' }}
                  {...register('username',{required:'Username is required'})}
                  error = {!!errors.username} 
                  helperText = {errors?.username?.message?.toString()} 
                />
              </FormControl>
              <FormControl>
                <TextField
                  placeholder="email"
                  label="email"
                  fullWidth
                  variant="outlined"
                  sx={{ ariaLabel: 'email' }}
                  {...register('email',
                    {required:'email is required',
                    pattern:{
                            value:/^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$/,
                            message:'Enter a valid email'
                        }},
                    )}
                  error = {!!errors.email} 
                  helperText = {errors?.email?.message?.toString()} 
                />
              </FormControl>
              <FormControl>
                <TextField
                  placeholder="password"
                  type="password"
                  label="password"
                  fullWidth
                  variant="outlined"
                  {...register('password',{
                    required:'Password is required',
                    pattern:{
                        value:/^(?=[^\d_].*?\d)\w(\w|[!@#$%]){7,20}/,
                        message:'enter a valid password'
                    }
                })}
                  error = {!!errors.password}
                  helperText = {errors?.password?.message?.toString()}
                />
              </FormControl>
              <LoadingButton
                disabled = {!isValid}
                type="submit"
                fullWidth
                loading={isSubmitting}
                variant="contained"
              >
                Register
              </LoadingButton>
              <Typography sx={{ textAlign: 'center' }}>
                Already have an account?{' '}
                <span>
                  <Link
                    to="/login"
                  >
                    Sign In
                  </Link>
                </span>
              </Typography>
            </Box>          
          </Container>
            </>
  );
}